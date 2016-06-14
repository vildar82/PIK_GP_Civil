using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.Kadastr
{
    public static class KadastrService
    {
        public static void OKSXML(Document doc)
        {
            Database db = doc.Database;
            // сборка кадастра
            var kadastrDll = Path.Combine(PIK_GP_Acad.Commands.CurDllDir, "Kadastr(x64)2015.dll");
            if (File.Exists(kadastrDll))
            {
                Assembly.LoadFrom(kadastrDll);
            }
            else
            {
                Inspector.AddError($"Не найдена программа кадастра - {kadastrDll}", System.Drawing.SystemIcons.Error);
                return;
            }

            // Загрузка блоков PB, PB1            
            PIK_GP_Acad.InsertBlock.LoadBlock(new List<string> { "PB", "PB1" }, db);

            // Запуск команды KPTXML            
            doc.SendStringToExecute("OKSXML ", false, true, true);
        }
    }
}
