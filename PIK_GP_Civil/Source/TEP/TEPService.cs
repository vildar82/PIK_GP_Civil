using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Gis.Map;

namespace PIK_GP_Civil.TEP
{
    public class TEPService
    {
        Document doc;
        Database db;
        Editor ed;

        public MapApplication MapApp { get { return HostMapApplicationServices.Application; } }

        public TEPService(Document doc)
        {
            this.doc = doc;
            db = doc.Database;
            ed = doc.Editor;
        }

        public void Calc ()
        {            
            var sel = ed.Select("\nВыбор:");
            var classifivators = GetClassificators(sel);
            // Группировка и суммирование
            var data = GetData(classifivators);
            // Таблица
            TableService ts = new TableService (doc);
            ts.Create(data);
        }

        private List<ITEPRow> GetData (List<IClassificator> classifivators)
        {
            List<ITEPRow> data = new List<ITEPRow> ();

            var groups = classifivators.GroupBy(g=>g.Name).OrderBy(o=>o.First().Index);
            foreach (var item in groups)
            {
                TEPRow row = new TEPRow (item.Key, item.ToList());
                data.Add(row);
            }
            return data;
        }

        private List<IClassificator> GetClassificators (List<ObjectId> ids)
        {
            List<IClassificator> classificators = new List<IClassificator> ();
            using (var t = db.TransactionManager.StartTransaction())
            {
                var map = MapApp.ActiveProject;
                foreach (var idEnt in ids)
                {
                    try
                    {
                        var tags = new StringCollection ();
                        var schemas = new StringCollection ();
                        map.ClassificationManager.GetAllTags(ref tags, ref schemas, idEnt);
                        if (tags.Count != 0)
                        {
                            var classificator = ClassFactory.Create(idEnt, tags, schemas);
                            classificators.Add(classificator);
                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                t.Commit();
            }
            return classificators;
        }
    }
}
