using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Civil.DatabaseServices.Styles;
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil;

namespace PIK_GP_Civil.Styles
{
    /// <summary>
    /// Загрузка стилей в текущий документ
    /// </summary>
    /// <param name="styleName"></param>
    /// <param name="dbTarget">Чертеж в которую загрузить стиль</param>
    /// <param name="findLoadedStyle">Функция поиска нужного стиля в шаблоне в дереве всех стилей</param>         
    public static class StyleManager
    {
        public static void LoadStyles(Database dbTarget, Func<StylesRoot, List<ObjectId>> findLoadedStyle,
            TemplateFileType templateType = TemplateFileType.GP, 
            StyleConflictResolverType mode = StyleConflictResolverType.Ignore)
        {
            var templateFile = GetTemplateFile(templateType);
            using (var dbTemplate = new Database(false, true))
            {
                dbTemplate.CloseInput(true);
                dbTemplate.ReadDwgFile(templateFile, FileShare.ReadWrite, true, "");

                using (var t = dbTemplate.TransactionManager.StartTransaction())
                {
                    var civilTemplate = CivilDocument.GetCivilDocument(dbTemplate);
                    var styleIds = findLoadedStyle(civilTemplate.Styles);
                    foreach (var styleId in styleIds)
                    {
                        var style = styleId.GetObject(OpenMode.ForRead) as StyleBase;
                        style.ExportTo(dbTarget, mode);
                    }                    

                    t.Commit();
                }
            }
        }       

        private static string GetTemplateFile(TemplateFileType templateType)
        {
            switch (templateType)
            {
                case TemplateFileType.GP:
                    return @"z:\Temp\Civil\GP\PIK_GP.dwt"; // @"z:\Civil3D_server\GP\PIK_GP.dwt";                    
                case TemplateFileType.KP:
                    return @"z:\Temp\Civil\GP_NR\PIK_GP_NR.dwt"; // @"z:\Civil3D_server\GP_NR\PIK_GP_NR.dwt";
            }
            return null;
        }        
    }
}
