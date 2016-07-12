using System;
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

namespace PIK_GP_Civil.TEP
{
    public class TEPService
    {
        static RXClass RXClassCurve = RXClass.GetClass(typeof(Curve));
        static RXClass RXClassHatch = RXClass.GetClass(typeof(Hatch));

        Document doc;
        Database db;
        Editor ed;
        ITableService ts;

        public MapApplication MapApp { get { return HostMapApplicationServices.Application; } }

        public TEPService(Document doc, ITableService tableService)
        {
            this.doc = doc;
            db = doc.Database;
            ed = doc.Editor;
            this.ts = tableService;
        }

        public void Calc ()
        {            
            var sel = ed.Select("\nВыбор:");
            var classifivators = GetClassificators(sel);
            // Группировка и суммирование
            CalcData(classifivators);
            // Таблица            
            var table = ts.Create();
            ts.Insert(table, doc);
        }

        private void CalcData (List<IClassificator> classificators)
        {
            var groups = classificators.GroupBy(g=>g.ClassType.TableName).OrderBy(o=>o.First().ClassType.Index).ToList();
            ts.CalcRows(groups);                        
        }

        private List<IClassificator> GetClassificators (List<ObjectId> ids)
        {
            List<IClassificator> classificators = new List<IClassificator> ();
            using (var t = db.TransactionManager.StartTransaction())
            {
                var map = MapApp.ActiveProject;
                foreach (var idEnt in ids)
                {
                    if (!idEnt.ObjectClass.IsDerivedFrom(RXClassCurve) && 
                        idEnt.ObjectClass != RXClassHatch)
                    {
                        continue;
                    }
                    
                    try
                    {
                        var tags = new StringCollection ();
                        var schemas = new StringCollection ();
                        map.ClassificationManager.GetAllTags(ref tags, ref schemas, idEnt);
                        if (tags.Count != 0)
                        {
                            var classificator = ClassFactory.Create(idEnt, tags, schemas, ts);
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
                    catch
                    {
                        //GetAllTags кидает исключение                   
                    }
                }
                t.Commit();
            }
            return classificators;
        }
    }
}
