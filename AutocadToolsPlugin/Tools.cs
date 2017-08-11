using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;


namespace AutocadToolsPlugin
{
    public class Tools
    {
        public static void CreateAutoNumerateText()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acDb = acDoc.Database;
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            var fixOpt = new PromptStringOptions("set prefix");
            var st = ed.GetString(fixOpt);
            var fixTextValue = st.StringResult;

            var optNotFix = new PromptIntegerOptions("Offset")
            {
                AllowNone = false,
                AllowZero = true,
                AllowNegative = false,
                DefaultValue = 0
            };
            var st1 = ed.GetInteger(optNotFix);
            var beginInt = st1.Value;

            var pPtOpts = new PromptPointOptions("Insert point\n");

            while (true)
            {
                using (var acTrans = acDb.TransactionManager.StartTransaction())
                {
                    var pPtRes = ed.GetPoint(pPtOpts);
                    var pos = pPtRes.Value;
                    if (pPtRes.Status == PromptStatus.Cancel) break;
                    beginInt = beginInt + 1;
                    CreateAcText(fixTextValue, beginInt, pos, acTrans);
                    acTrans.Commit();
                }
            }
            
        }
        
        public static void CreateAcText(string fix, int number, Point3d pos, Transaction acTrans)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acDb = acDoc.Database;
            var blockTable = acTrans.GetObject(acDb.BlockTableId, OpenMode.ForRead) as BlockTable;
            var blockTableRec = acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
            var text = new DBText
            {
                Position = pos,
                TextString = fix + Convert.ToString(number)
            };

            blockTableRec.AppendEntity(text);
            acTrans.AddNewlyCreatedDBObject(text, true);
        }

        /// <summary>
        /// Gets MTetx and DBText objects from autocad's selection, and convert to string
        /// </summary>
        /// <returns>string</returns>
        public static string GetTextBySelection()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            var result = "";
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var selecttionPrompt = editor.GetSelection();

                if (selecttionPrompt.Status != PromptStatus.OK) return result;
                
                foreach (SelectedObject selectedObject in selecttionPrompt.Value)
                {
                    if (selectedObject == null) continue;
                    var dbText = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as DBText;

                    if (dbText != null)
                    {
                        result += dbText.TextString;
                    }
                    else
                    {
                        var mText = transaction.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as MText;
                        if (mText == null) continue;
                        result += mText.Contents;
                    }
                }
            }
            return result;
        }
        
        /// <summary>
        /// Gets all digits(i.e 123 0.123 0,123 .123 ) from txt
        /// </summary>
        /// <param name="txt">string, for example r=0.256kg</param>
        /// <returns>list digits</returns>
        public static List<float> GetListDigits(string txt)
        {
            txt = Regex.Replace(txt, @",", ".");
            return (from Match match in Regex.Matches(txt, @"(\d+[\.]\d+)|([\.]\d+)|\d+")
                select Convert.ToSingle(match.Value, new CultureInfo("en-US"))).ToList();
        }
    }
}