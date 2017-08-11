using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;


namespace AutocadToolsPlugin
{
    public class Commands : IExtensionApplication
    {
        public void Initialize()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(Tools.GetText());
            ed.WriteMessage("another text yet");
        }

        public void Terminate()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Terminate AutoNumerate plugin");
        }

        [CommandMethod("NUMERABLETEXT")]
        public void AutoNumerateCommand()
        {
            Tools.CreateAutoNumerateText();
        }
        
        [CommandMethod("НУМЕРАШКА")]
        public void AutoNumerateCommandAlias()
        {
            AutoNumerateCommand();
        }

        [CommandMethod("SUMMARIZETEXT", CommandFlags.UsePickSet)]
        public void SumDigitsFromTextObjects()
        {
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            var digits = Tools.GetListDigits(Tools.GetTextBySelection());
            var result = 0.0;
            var cInfo = new CultureInfo("ru-RU");
            foreach (var digit in digits)
            {
                result = result + digit;
                editor.WriteMessage(string.Format(">>> {0}\n",digit.ToString(cInfo)));
                         }
            var sum = Math.Round (result, 3);
            editor.WriteMessage(string.Format("total: {0}", sum.ToString(cInfo)));
        }

        [CommandMethod("СКЛАДЫВАЛКА", CommandFlags.UsePickSet)]
        public void SumDigitsFromTextObjectsAlias()
        {
            SumDigitsFromTextObjects();
        }       
    }
}