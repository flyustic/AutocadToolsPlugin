using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;


namespace AutocadToolsPlugin
{
    public class Tools
    {
        /// <summary>
        /// Draw DBText in autocad
        /// </summary>
        /// <param name="txt">DBText object</param>
        public static void DrawDBtext(DBText text)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var tableRec =
                    tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                tableRec.AppendEntity(text);
                tr.AddNewlyCreatedDBObject(text, true);
                tr.Commit();
            }
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
                select Convert.ToSingle(match.Value, CultureInfo.InvariantCulture)).ToList();
        }
    }
}