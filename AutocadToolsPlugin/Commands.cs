using System;
using System.Globalization;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AutocadToolsPlugin
{
    public class Commands : IExtensionApplication
    {
        public void Initialize()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Start AutocadToolsPlugin");
        }

        public void Terminate()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Terminate AutocadToolsPlugin");
        }

        [CommandMethod("NUMERABLETEXT")]
        public void AutoNumerateCommand()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var prefix = ed.GetString(new PromptStringOptions("set prefix")).StringResult;
            var offsetOpt = new PromptIntegerOptions("Offset")
            {
                AllowNone = false,
                AllowZero = true,
                AllowNegative = false,
                DefaultValue = 1
            };
            var offset = ed.GetInteger(offsetOpt).Value;
            var pPtOpts = new PromptPointOptions("\ninsert point:");

            while (true)
            {
                var pPtRes = ed.GetPoint(pPtOpts);
                if (pPtRes.Status == PromptStatus.Cancel) break;
                var position = pPtRes.Value;
                var text = new DBText
                {
                    Position = position,
                    TextString = string.Format("{0}{1}", prefix, offset)
                };
                Tools.DrawDBtext(text);
                offset = offset + 1;
                pPtOpts.Message = "\ninsert next point:";
            }
        }
        
        [CommandMethod("НУМЕРАШКА")]
        public void AutoNumerateCommandAlias()
        {
            AutoNumerateCommand();
        }

        [CommandMethod("SUMMARIZETEXT", CommandFlags.UsePickSet)]
        public void SumDigitsFromTextObjects()
        {
            
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            var totalText = " ";
            var selecttionPrompt = editor.GetSelection();
            if (selecttionPrompt.Status != PromptStatus.OK) return;
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject selectedObject in selecttionPrompt.Value)
                {
                    if (selectedObject == null) continue;
                    var dbText = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as DBText;

                    if (dbText != null)
                    {
                        totalText = totalText + " " + dbText.TextString;
                    }
                    else
                    {
                        var mText = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as MText;
                        if (mText == null) continue;
                        totalText = totalText + " " + mText.Text;
                    }
                }
            }
            var digits = Tools.GetListDigits(totalText);
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

        [CommandMethod("FIELDSBYTEXTS", CommandFlags.UsePickSet)]
        public void GetFormatForManyFields()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            var text = "";
            const string separator = ";";
            const string template = "%<\\AcObjProp Object(%<\\_ObjId {0}>%).TextString>%";
            const string templateForLen = "%<\\AcObjProp.16.2 Object(%<\\_ObjId {0} >%).Length \\f \"%lu2%pr1%ds44%ct8[0.001]\" >%";

            var selecttionPrompt = editor.GetSelection();
            if (selecttionPrompt.Status != PromptStatus.OK) return;
            
            using (var transaction = db.TransactionManager.StartTransaction())
            {

                foreach (SelectedObject selectedObject in selecttionPrompt.Value)
                {
                    if (selectedObject == null) continue;
                    var dbText = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as DBText;
                    if (dbText != null)
                    {
                        text += string.Format(template, dbText.ObjectId.OldIdPtr) + separator;
                    }
                    else
                    {
                        var mText = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as MText;
                        if (mText == null) continue;
                        
                        text += string.Format(template, mText.ObjectId.OldIdPtr) + separator;
                    }
                }
            }
            editor.WriteMessage(string.Format("============================\n{0}\n============================", text));
        }
        
        [CommandMethod("ФИЛДЫ", CommandFlags.UsePickSet)]
        public void GetFormatForManyFieldsAlias()
        {
            GetFormatForManyFields();
        }

        [CommandMethod("FIELDSBYTEXTSLEN", CommandFlags.UsePickSet)]
        public void GetFormatForManyFieldsLen()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            var text = "";
            const string separator = ";";
            const string template = "%<\\AcObjProp.16.2 Object(%<\\_ObjId {0} >%).Length \\f \"%lu2%pr1%ds44%ct8[0.001]\" >%";

            var selecttionPrompt = editor.GetSelection();
            if (selecttionPrompt.Status != PromptStatus.OK) return;

            using (var transaction = db.TransactionManager.StartTransaction())
            {

                foreach (SelectedObject selectedObject in selecttionPrompt.Value)
                {
                    if (selectedObject == null) continue;
                    var ent = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as Entity;
                    if (ent as Line != null || ent as Polyline != null)
                    {
                        text += string.Format(template, ent.ObjectId.OldIdPtr) + separator;
                    }
                }
            }
            editor.WriteMessage(string.Format("============================\n{0}\n============================", text));
        }

        [CommandMethod("ФИЛДЫЛЕН", CommandFlags.UsePickSet)]
        public void GetFormatForManyFieldsLenAlias()
        {
            GetFormatForManyFieldsLen();
        }
    }
}