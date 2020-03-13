// Skeleton written by Joe Zachary for CS 3500, January 2019
// Sungyeon Han, u0970346

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Formulas.TokenType;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operation + and -
    /// are not allowed.)
    /// </summary>
    public class Formula
    {
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>

        private List<String> tokens;
        private HashSet<string> normVar;
        private string firstTok = null;
        private double chkDigit = 0;
        private double firstpopValue = 0;
        private double secondpopValue = 0;

        public Formula(String formula) : this(formula, s => s, s => true)
        {
        }

        public Formula(String formula, Normalizer normalizer, Validator validator)
        {
            if (formula == null || normalizer == null || validator == null)
                throw new ArgumentNullException("Paramaters cannot be null!");

            int leftParen = 0;
            int rightParen = 0;
 
            this.tokens = new List<string>();
            String varNormal = null;
            normVar = new HashSet<string>();

            string firstTok = null;

            Boolean chkLoop = true;

            foreach (Token token in GetTokens(formula))
            {
                if (token.Equals(null))
                    throw new FormulaFormatException("Please enter one token");

                if (token.Text == "(")
                    leftParen++;
                else if (token.Text == ")")
                    rightParen++;

                if (chkLoop == true)
                {
                    if (!Regex.IsMatch(token.Text, @"^[a-zA-Z][0-9a-zA-Z]*$") && token.Text != "(" && !Double.TryParse(token.Text, out chkDigit))
                    {
                        throw new FormulaFormatException("Invalid input");
                    }
                }

                else
                {
                    if (firstTok == "(" || Regex.IsMatch(firstTok, @"^[\+\-*/]$"))
                        if (!(Regex.IsMatch(token.Text, @"^[a-zA-Z][0-9a-zA-Z]*$") || (token.Text == "(") || Double.TryParse(token.Text, out chkDigit)))
                            throw new FormulaFormatException("Invalid input after a left parenthesis or operator");

                    if (Regex.IsMatch(firstTok, @"^[a-zA-Z][0-9a-zA-Z]*$") || firstTok == ")" || Double.TryParse(firstTok, out chkDigit))
                        if (!(Regex.IsMatch(token.Text, @"^[\+\-*/]$") || token.Text == ")"))
                            throw new FormulaFormatException("Invalid input after a number, a variable, or right parenthesis.");
                }

                if (Regex.IsMatch(token.Text, @"^[a-zA-Z][0-9a-zA-Z]*$"))
                {
                    if (!validator(normalizer(token.Text)))
                        throw new FormulaFormatException("Invalid normalized variable");
                    else 
                    {
                        varNormal = normalizer(token.Text);
                        tokens.Add(varNormal);
                        normVar.Add(varNormal);
                    }
                }
                else
                {
                    tokens.Add(token.Text);
                }

                firstTok = token.Text;

                chkLoop = false;
            }

            if (tokens.Count == 0)
                throw new FormulaFormatException("Invalid tokens");

            if (!Regex.IsMatch(firstTok, @"^[a-zA-Z][0-9a-zA-Z]*$") && firstTok != ")" && !Double.TryParse(firstTok, out chkDigit))
                throw new FormulaFormatException("Formula has invliad input");

            if (rightParen != leftParen)
                throw new FormulaFormatException("The number of left parentheses does not equal the number of right parentheses.");
        }

        public ISet<String> GetVariables()
        {
            HashSet<string> getVar = new HashSet<string>(normVar);
            return getVar;
        }

        /// <summary>
        /// Override the ToString() method so that it returns a string version of the Formula (in normalized form).  
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string formula = "";
            for (int i = 0; i < tokens.Count; i++)
            {
                formula += tokens[i];
            }
            return formula;
        }

        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>


        public double Evaluate(Lookup lookup)
        {
            if (lookup == null)
            {
                throw new ArgumentNullException("Paramaters can't be null");
            }

            Stack<string> operators = new Stack<string>();
            Stack<string> values = new Stack<string>();

            double chkNum = 0;
            string firstTok = null;

            foreach (string token in tokens)
            {
                if (Double.TryParse(token, out chkNum) || Regex.IsMatch(token, @"^[a-zA-Z][0-9a-zA-Z]*$"))
                {
                    if (!Double.TryParse(token, out chkNum))
                    {
                        try
                        {
                            double value = lookup(token);
                            chkNum = value;
                        }
                        catch (UndefinedVariableException e)
                        {
                            throw new FormulaEvaluationException("undefined");
                        }
                    }

                    if ((operators.Count != 0) && (operators.Peek() == "*" || operators.Peek() == "/"))
                    {
                        string popOperator = operators.Pop();
                        double popValue = 0;
                        Double.TryParse(values.Pop(), out popValue);

                        if (popOperator == "/")
                            if (chkNum == 0)
                                throw new FormulaEvaluationException("Input cannot be zero");
                            else
                                values.Push((popValue / chkNum).ToString());
                        else if (popOperator == "*")
                        {
                            values.Push((popValue * chkNum).ToString());
                        }

                    }
                    else
                    {
                        values.Push(chkNum.ToString());
                    }
                }

                if (token == "+" || token == "-")
                {
                    if ((operators.Count != 0) && (operators.Peek() == "+" || operators.Peek() == "-"))
                    {
                        string popOperator = operators.Pop();
                        Double.TryParse(values.Pop(), out firstpopValue);
                        Double.TryParse(values.Pop(), out secondpopValue);

                        if (popOperator == "+")
                            values.Push((firstpopValue + secondpopValue).ToString());
                        else if (popOperator == "-")
                            values.Push((secondpopValue - firstpopValue).ToString());
                    }

                    operators.Push(token);
                }

                if (token == "*" || token == "/")
                {
                    operators.Push(token);
                }

                if (token == "(")
                {
                    operators.Push(token);
                }

                if (token == ")")
                {
                    if ((values.Count > 1) && (operators.Peek() == "+" || operators.Peek() == "-"))
                    {
                        string popOperator = operators.Pop();
                        Double.TryParse(values.Pop(), out firstpopValue);
                        Double.TryParse(values.Pop(), out secondpopValue);

                        if (popOperator == "+")
                            values.Push((firstpopValue + secondpopValue).ToString());
                        else if (popOperator == "-")
                            values.Push((secondpopValue - firstpopValue).ToString());
                    }


                    operators.Pop();

                    if ((values.Count > 1) && (operators.Peek() == "*" || operators.Peek() == "/"))
                    {
                        string popOperator = operators.Pop();
                        Double.TryParse(values.Pop(), out firstpopValue);
                        Double.TryParse(values.Pop(), out secondpopValue);

                        if (popOperator == "*")
                            values.Push((firstpopValue * secondpopValue).ToString());
                        else if (popOperator == "/")
                        {
                            if (firstpopValue != 0)
                                values.Push((secondpopValue / firstpopValue).ToString());
                            else
                                throw new FormulaEvaluationException("Input cannot be 0.");
                        }
                    }
                }

                firstTok = token;
            }

            double chkDigit = 0.0;

            if (operators.Count != 0)
            {
                if ((values.Count > 1) && (operators.Peek() == "+" || operators.Peek() == "-"))
                {
                    string popOperator = operators.Pop();
                    Double.TryParse(values.Pop(), out firstpopValue);
                    Double.TryParse(values.Pop(), out secondpopValue);

                    if (popOperator == "+")
                        chkDigit = firstpopValue + secondpopValue;
                    else if (popOperator == "-")
                        chkDigit = secondpopValue - firstpopValue;
                }
            }
            else if (operators.Count == 0)
            {
                Double.TryParse(values.Peek(), out chkDigit);
            }
            return chkDigit;
        }

        public struct Token
        {
            public string Text { get; }
            public TokenType Type { get; }
            public Token(String text, TokenType type) : this()
            {
                Text = text;
                Type = type;
            }

        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Each token is described by a
        /// Tuple containing the token's text and TokenType.  There are no empty tokens, and no
        /// token contains white space.
        /// </summary>
        private static IEnumerable<Token> GetTokens(String formula)
        {
            // Patterns for individual tokens.
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";

            // NOTE:  I have added white space to this regex to make it more readable.
            // When the regex is used, it is necessary to include a parameter that says
            // embedded white space should be ignored.  See below for an example of this.
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall token pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.
            String tokenPattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5}) | (.)",
                                            spacePattern, lpPattern, rpPattern, opPattern, varPattern, doublePattern);

            // Create a Regex for matching tokens.  Notice the second parameter to Split says 
            // to ignore embedded white space in the pattern.
            Regex r = new Regex(tokenPattern, RegexOptions.IgnorePatternWhitespace);

            // Look for the first match
            Match match = r.Match(formula);

            // Start enumerating tokens
            while (match.Success)
            {
                // Ignore spaces
                if (!match.Groups[1].Success)
                {
                    // Holds the token's type
                    TokenType type;

                    if (match.Groups[2].Success)
                    {
                        type = LParen;
                    }
                    else if (match.Groups[3].Success)
                    {
                        type = RParen;
                    }
                    else if (match.Groups[4].Success)
                    {
                        type = Oper;
                    }
                    else if (match.Groups[5].Success)
                    {
                        type = Var;
                    }
                    else if (match.Groups[6].Success)
                    {
                        type = Number;
                    }
                    else if (match.Groups[7].Success)
                    {
                        type = Invalid;
                    }
                    else
                    {
                        // We shouldn't get here
                        throw new InvalidOperationException("Regular exception failed in GetTokens");
                    }

                    // Yield the token
                    yield return new Token(match.Value, type);
                }

                // Look for the next match
                match = match.NextMatch();
            }
        }
    }

    /// <summary>
    /// Identifies the type of a token.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Left parenthesis
        /// </summary>
        LParen,

        /// <summary>
        /// Right parenthesis
        /// </summary>
        RParen,

        /// <summary>
        /// Operator symbol
        /// </summary>
        Oper,

        /// <summary>
        /// Variable
        /// </summary>
        Var,

        /// <summary>
        /// Double literal
        /// </summary>
        Number,

        /// <summary>
        /// Invalid token
        /// </summary>
        Invalid
    };

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string var);
    public delegate string Normalizer(string s);
    public delegate bool Validator(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}
