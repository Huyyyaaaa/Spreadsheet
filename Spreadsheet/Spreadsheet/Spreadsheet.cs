using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Dependencies;
using Formulas;

namespace SS
{
    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string s is a valid cell name if and only if it consists of one or more letters, 
    /// followed by a non-zero digit, followed by zero or more digits.
    /// 
    /// For example, "A15", "a15", "XY32", and "BC7" are valid cell names.  On the other hand, 
    /// "Z", "X07", and "hello" are not valid cell names.
    /// 
    /// A spreadsheet contains a unique cell corresponding to each possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  The distinction is
    /// important, and it is important that you understand the distinction and use
    /// the right term when writing code, writing comments, and asking questions.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In an empty spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError.
    /// The value of a Formula, of course, can depend on the values of variables.  The value 
    /// of a Formula variable is the value of the spreadsheet cell it names (if that cell's 
    /// value is a double) or is undefined (otherwise).  If a Formula depends on an undefined
    /// variable or on a division by zero, its value is a FormulaError.  Otherwise, its value
    /// is a double, as specified in Formula.Evaluate.
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        private DependencyGraph dg;
        private Dictionary<string, Cell> cells;

        public override bool Changed
        {
            get;

            protected set;
        }

        private Regex IsValid { get; set; }

        public Spreadsheet() : this(new Regex(""))
        {

        }
        public Spreadsheet(Regex valid)
        {
            this.dg = new DependencyGraph();
            this.cells = new Dictionary<string, Cell>();
            IsValid = valid;
            Changed = false;
        }

        /// If there's a problem reading source, throws an IOException.
        ///
        /// Else if the contents of source are not consistent with the schema in Spreadsheet.xsd, 
        /// throws a SpreadsheetReadException.  
        ///
        /// Else if the IsValid string contained in source is not a valid C# regular expression, throws
        /// a SpreadsheetReadException.  (If the exception is not thrown, this regex is referred to
        /// below as oldIsValid.)
        ///
        /// Else if there is a duplicate cell name in the source, throws a SpreadsheetReadException.
        /// (Two cell names are duplicates if they are identical after being converted to upper case.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a 
        /// SpreadsheetReadException.  (Use oldIsValid in place of IsValid in the definition of 
        /// cell name validity.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a
        /// SpreadsheetVersionException.  (Use newIsValid in place of IsValid in the definition of
        /// cell name validity.)
        ///
        /// Else if there's a formula that causes a circular dependency, throws a SpreadsheetReadException. 
        ///
        /// Else, create a Spreadsheet that is a duplicate of the one encoded in source except that
        /// the new Spreadsheet's IsValid regular expression should be newIsValid.
        public Spreadsheet(TextReader source, Regex newIsValid) {
            this.dg = new DependencyGraph();
            this.cells = new Dictionary<string, Cell>();

            Regex checkValid = newIsValid;

            XmlSchemaSet schema = new XmlSchemaSet();

            schema.Add(null, "Spreadsheet.xsd");

            XmlReaderSettings xmlSetting = new XmlReaderSettings();
            xmlSetting.ValidationType = ValidationType.Schema;
            xmlSetting.Schemas = schema;
            xmlSetting.ValidationEventHandler += (object sender, ValidationEventArgs e) => throw new SpreadsheetReadException("not valid");

            using (XmlReader xmlReader = XmlReader.Create(source, xmlSetting))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.IsStartElement())
                    {
                        if (xmlReader.Name == "cell")
                        {
                            if (cells.ContainsKey(xmlReader["name"]))
                            {
                                throw new SpreadsheetReadException("duplicate cell in the source");
                            }

                            IsValid = checkValid;
                            try
                            {
                                this.SetContentsOfCell(xmlReader["name"], xmlReader["contents"]);
                            }
                            catch (FormulaFormatException e)
                            {
                                throw new SpreadsheetReadException("invalid");
                            }
                            catch (CircularException e)
                            {
                                throw new SpreadsheetReadException("invalid");
                            }
                            catch (InvalidNameException e)
                            {
                                throw new SpreadsheetReadException("invalid");
                            }


                            IsValid = newIsValid;
                            try
                            {
                                this.SetContentsOfCell(xmlReader["name"], xmlReader["contents"]);
                            }
                            catch (FormulaFormatException e)
                            {
                                throw new SpreadsheetVersionException("invalid");
                            }
                            catch (CircularException e)
                            {
                                throw new SpreadsheetVersionException("invalid");
                            }
                            catch (InvalidNameException e)
                            {
                                throw new SpreadsheetVersionException("invalid");
                            }
                        }
                        else if (xmlReader.Name == "spreadsheet")
                        {
                            try
                            {
                                checkValid = new Regex(xmlReader["IsValid"]);
                            }
                            catch (ArgumentException e)
                            {
                                throw new SpreadsheetReadException("invalid");
                            }
                        } 
                    }
                }
            }
        }

        private class Cell {

            public Object contents { get; private set; }
            public Object value { get; set; }

            /// <summary>
            /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
            /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
            /// in the grid.)
            /// 
            /// If a cell's contents is a string, its value is that string.
            /// 
            /// If a cell's contents is a double, its value is that double.
            /// 
            /// If a cell's contents is a Formula, its value is either a double or a FormulaError.
            /// The value of a Formula, of course, can depend on the values of variables.  The value 
            /// of a Formula variable is the value of the spreadsheet cell it names (if that cell's 
            /// value is a double) or is undefined (otherwise).  If a Formula depends on an undefined
            /// variable or on a division by zero, its value is a FormulaError.  Otherwise, its value
            /// is a double, as specified in Formula.Evaluate.
            public Cell(string name)
            {
                contents = name;
                value = name;
            }

            public Cell(double name)
            {
                contents = name;
                value = name;
            }

            public Cell(Formula name)
            {
                contents = name;
                value = name;
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            // check if text is null
            if (ReferenceEquals(name, null))
            {
                throw new ArgumentNullException();
            }

            //check if name is null or invallid
            if (!Regex.IsMatch(name, IsValid.ToString()) || ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$"))
            {
                throw new InvalidNameException();
            }
            
            // Check if dictionary contains name
            if (!cells.TryGetValue(name.ToUpper(), out Cell value))
                return "";
            else
                return value.contents; // Return value from contents
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
           return new List<string>(cells.Keys);
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            // check if text is null
            if (ReferenceEquals(number, null))
            {
                throw new ArgumentNullException();
            }

            //check if name is null or invalid
            if (!Regex.IsMatch(name, IsValid.ToString()) || ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$"))
            {
                throw new InvalidNameException();
            }

            // Check if cell does not contain name, if so then add new key for that value
            //if cell does contain name, then replace key with new value
            if (!cells.ContainsKey(name))
                cells.Add(name, new Cell(number));
            else
                cells[name] = new Cell(number);

            //replace dependent of name with empty hash set
            dg.ReplaceDependees(name, new HashSet<String>());
            foreach (string values in new HashSet<string>(GetCellsToRecalculate(name)))
            {
                if (cells[values].contents is Formula)
                {
                    try
                    {
                        Formula f = (Formula)cells[values].contents;
                        double value = f.Evaluate((Lookup)LookupValue);
                        cells[values].value = value;
                    }
                    catch (Exception e)
                    {
                        cells[values].value = new FormulaError();
                    }
                }
            }
            // initialize hashset and call GetCellsToRecalculate to return name
            return new HashSet<string>(GetCellsToRecalculate(name));
        }
        
        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        /// 
        protected override ISet<string> SetCellContents(string name, string text) 
        {
            // check if name is null or invalid
            if (!Regex.IsMatch(name, IsValid.ToString()) || ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$"))
            {
                throw new InvalidNameException();
            }

            // check if text is null
            if (ReferenceEquals(text, null))
            {
                throw new ArgumentNullException();
            }

            // check if cell contains name, and if does not then add new key for that value.
            // if it does contain name, then replace key with new value
            if (!cells.ContainsKey(name))
                cells.Add(name, new Cell(text)); 
            else
                cells[name] = new Cell(text);

            //check is contents is empty, and then remove name
            string content = (string)cells[name].contents;
            if (content.Equals(""))
                cells.Remove(name);

            // replace dependents of name with empty hash set
            dg.ReplaceDependees(name, new HashSet<String>());

            // initialize hashset and call GetCellsToRecalculate to return name
            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        /// <summary>
        /// Requires that all of the variables in formula are valid cell names.
        /// 
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            // check if name is null or invalid
            if (!Regex.IsMatch(name, IsValid.ToString()) || ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$"))
            {
                throw new InvalidNameException();
            }

            IEnumerable<String> dependees = dg.GetDependees(name);

            // replace dependents of name with the formula variables
            dg.ReplaceDependees(name, formula.GetVariables());

            //check if dependency graph produces circular dependency
            try
            {
                HashSet<String> hsDependees = new HashSet<String>(GetCellsToRecalculate(name));

                Cell cell = new Cell(formula);
                cell.value = formula;

                // if cell does not contain name, add new key for the value.
                // if it does contain name, then replace key with the value
                if (!cells.ContainsKey(name))
                    cells.Add(name, new Cell(formula));
                else
                    cells[name] = new Cell(formula);

                foreach (string values in new HashSet<String>(GetCellsToRecalculate(name)))
                {
                    if (!(cells[values].contents is Formula))
                    {
                        cell.value = formula.Evaluate((Lookup)LookupValue);
                    }
                    else
                    {
                        try
                        {
                            Formula f = (Formula)cells[values].contents;
                            double value = f.Evaluate((Lookup)LookupValue);
                            cells[values].value = value;
                        }
                        catch (Exception e)
                        {
                            cells[values].value = new FormulaError();
                        }
                    }
                }

                return new HashSet<String>(GetCellsToRecalculate(name));
            }
            // if exception is caught, keep old dependents
            catch (CircularException e)
            {
                dg.ReplaceDependees(name, dependees);
                throw new CircularException();
            }
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            //check if name is invalid
            if (ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$"))
                throw new InvalidNameException();
            
            // return hash set using GetDependents
            return dg.GetDependents(name);
        }

        public override void Save(TextWriter dest)
        {
            try
            {
                XmlWriterSettings xmlSetting = new XmlWriterSettings();
                xmlSetting.Indent = true;
                XmlWriter writer = XmlWriter.Create(dest);
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");               
                writer.WriteAttributeString("IsValid", IsValid.ToString());
                using (writer)
                {
                    foreach (string cell in new List<string>(cells.Keys))
                    {
                        writer.WriteStartElement("cell");                        
                        writer.WriteAttributeString("name", cell);                                              

                        if (cells[cell].contents is Formula)
                        {
                            string cell_contents = "=" + cells[cell].contents.ToString();
                            writer.WriteAttributeString("contents", cell_contents);
                        }
                        else if (cells[cell].contents is double)
                        {
                            string cell_contents = cells[cell].contents.ToString();
                            writer.WriteAttributeString("contents", cell_contents);
                        }
                        else
                        {   
                            string cell_contents = (string)cells[cell].contents;
                            writer.WriteAttributeString("contents", cell_contents);
                        }
                        writer.WriteEndElement(); 
                    }
                    writer.WriteEndElement();               
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
                Changed = false;
            }
            catch (IOException e)
            {
                throw new IOException(e.ToString());
            }
            catch (XmlException e)
            {
                throw new IOException(e.ToString());
            }
        }

        public override object GetCellValue(string name)
        {
            // if name is null or invalid, throw exception
            if (!Regex.IsMatch(name, IsValid.ToString()) || ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$"))
                throw new InvalidNameException();

            name = name.ToUpper();

            //return value of cell
            if (!cells.TryGetValue(name, out Cell cell))
                return "";
            else
                return cell.value;
        }

        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            //check if null
            if (ReferenceEquals(content, null))
                throw new ArgumentNullException();

            //check is null or invalid
            if (ReferenceEquals(name, null) || !Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$") || !Regex.IsMatch(name, IsValid.ToString()))
                throw new InvalidNameException();

            name = name.ToUpper();

            //check if it's a valid name and not null
            
            if (content.Length > 0 && content[0] == '=')
            {
                Changed = true;
                Formula f = new Formula(content.Substring(1, content.Length - 1), s => s.ToUpper(), s => Regex.IsMatch(s, IsValid.ToString()));
                return new HashSet<String>(SetCellContents(name, f));
            }
            else if (Regex.IsMatch(content, "^(-?)(0|([1-9][0-9]*))(\\.[0-9]+)?$"))
            {
                Changed = true;
                double check = Double.Parse(content);
                return SetCellContents(name, check);
            }
            else
            {
                Changed = true;
                return new HashSet<String>(SetCellContents(name, content));
            }
        }

        //used for functions and to return cell value
        public double LookupValue(string s)
        {
            bool check = cells.TryGetValue(s, out Cell cell);
            if (check == false)
            {
                throw new UndefinedVariableException("");
            }
            else
            {
                if (!(cell.value is double))
                {
                    throw new UndefinedVariableException("");
                }
                else
                {
                    return (double)cell.value;
                }
            }
        }
    }
}
