using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using System.Collections.Generic;
using Formulas;
using System.IO;
using System.Text.RegularExpressions;

namespace SpreadSheetTests
{
    [TestClass]
    public class SpreadSheetTests
    {
        /// <summary>
        /// Test GetNamesOfAllNonemptyCells
        /// </summary>
        [TestMethod]
        public void test1()
        {
            HashSet<string> set = new HashSet<string>(new Spreadsheet().GetNamesOfAllNonemptyCells());
            Assert.AreEqual(0, set.Count);
        }

        /// <summary>
        /// Test GetCellContents
        /// </summary>
        [TestMethod]
        public void test2()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("x", "y");
            Assert.AreEqual("y", sheet.GetCellContents("x"));
        }

        /// <summary>
        /// Test GetCellContents when parameter is null
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void test3()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell(null, "1");
        }

        /// <summary>
        /// Test SetContentsOfCell when parameter is  null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void test4()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell(null, "x");

        }

        /// <summary>
        /// Test GetCellContents when parameter is an empty string
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void test6()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("x", "y");
            Assert.AreEqual("y", sheet.GetCellContents("x"));
            sheet.GetCellContents("");
        }

        /// <summary>
        /// InvalidNameException Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void test7()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("3x", "7");
        }

        /// <summary>
        /// InvalidNameException Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void test8()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("", "7");
        }

        /// <summary>
        /// InvalidNameException Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void test9()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("14", "7");
        }

        /// <summary>
        /// ArgumentNullException Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void test10()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            string test = null;
            sheet.SetContentsOfCell("x", test);
        }

        /// <summary>
        /// Test GetNamesOfAllNonemptyCells
        /// </summary>
        [TestMethod()]
        public void test17()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            Assert.IsFalse(sheet.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void test18()
        {
            AbstractSpreadsheet sheet1 = new Spreadsheet();
            AbstractSpreadsheet sheet2 = new Spreadsheet();
            sheet1.SetContentsOfCell("A1", "x");
            Assert.IsTrue(sheet1.Changed);
            Assert.IsFalse(sheet2.Changed);
        }

        [TestMethod()]
        public void test19()
        {
            try
            {
                AbstractSpreadsheet ss = new Spreadsheet(new StringReader("Hello world"), new Regex(""));
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }

    }
}