using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Gis.Map;
using Autodesk.Gis.Map.Classification;
using Autodesk.Gis.Map.Project;

namespace PIK_GP_Civil.FCS
{
    public class FCService
    {
        static RXClass RXClassCurve = RXClass.GetClass(typeof(Curve));
        static RXClass RXClassHatch = RXClass.GetClass(typeof(Hatch));

        Document doc;
        Database db;
        Editor ed;
        ITableService tableService;
        IClassTypeService classService;

        public static MapApplication MapApp { get { return HostMapApplicationServices.Application; } }

        public FCService(Document doc, ITableService tableService, IClassTypeService classService)
        {
            this.doc = doc;
            db = doc.Database;
            ed = doc.Editor;
            this.tableService = tableService;
            this.classService = classService;          
        }

        public static T GetPropertyValue<T> (string name, List<FCProperty> props, ObjectId idEnt, bool isRequired)
        {
            T resVal = default(T);
            var prop = props.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
            {
                if (isRequired)
                {
                    Inspector.AddError($"Не определен параметр {name}", idEnt, System.Drawing.SystemIcons.Error);
                }
            }
            else
            {
                try
                {
                    resVal = (T)Convert.ChangeType(prop.Value, typeof(T));
                }
                catch
                {
                    Inspector.AddError($"Недопустимый тип значения параметра '{name}'= {prop.Value.ToString()}.", 
                        idEnt, System.Drawing.SystemIcons.Error);
                }
            }
            return resVal;
        }        

        public void Calc ()
        {            
            var sel = ed.Select("\nВыбор:");
            var classifivators = GetClassificators(sel);
            // Группировка и суммирование
            CalcData(classifivators);
            // Таблица            
            var table = tableService.Create();
            tableService.Insert(table, doc);
        }

        private void CalcData (List<IClassificator> classificators)
        {
            var groups = classificators.GroupBy(g=>g.ClassType.TableName).OrderBy(o=>o.First().ClassType.Index).ToList();
            tableService.CalcRows(groups);                        
        }

        private List<IClassificator> GetClassificators (List<ObjectId> ids)
        {
            List<IClassificator> classificators = new List<IClassificator> ();
            using (var t = db.TransactionManager.StartTransaction())
            {
                foreach (var idEnt in ids)
                {
                    if (!idEnt.ObjectClass.IsDerivedFrom(RXClassCurve) &&
                        idEnt.ObjectClass != RXClassHatch)
                    {
                        continue;
                    }


                    var tags  = GetAllTags(idEnt);
                    if (tags.Any())
                    {
                        var classificator = ClassFactory.Create(idEnt, tags, classService);
                        if (classificator == null)
                        {
                            Inspector.AddError($"Пропущен объект класса - {string.Join(",", tags)}", idEnt, System.Drawing.SystemIcons.Warning);
                        }
                        else
                        {
                            classificators.Add(classificator);
                        }
                    }
                }
                t.Commit();
            }
            return classificators;
        }

        public static IEnumerable<string> GetAllTags (ObjectId idEnt)
        {
            StringCollection schemas = new StringCollection();
            StringCollection tags = new StringCollection();
            try
            {
                MapApp.ActiveProject.ClassificationManager.GetAllTags(ref tags, ref schemas, idEnt);
            }
            catch { }
            var res = tags.Cast<string>();
            return res;
        }

        public static List<FCProperty> GetProperties (ObjectId idEnt)
        {
            FeatureClassPropertyCollection props = new FeatureClassPropertyCollection();
            ArrayList values = new ArrayList();
            MapApp.ActiveProject.ClassificationManager.GetProperties(props, values, idEnt);
            List<FCProperty> res = FCProperty.GetProperties(props, values);
            return res;
        }
    }
}
