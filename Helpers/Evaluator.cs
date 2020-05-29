using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

public class Evaluator
{
    private parser mParser;
    private object mExtraFunctions;

    public Evaluator()
    {
        mParser = new parser(this);
    }

    private enum eTokenType
    {
        none,
        end_of_formula,
        operator_plus,
        operator_minus,
        operator_mul,
        operator_div,
        operator_exponent,
        operator_percent,
        open_parenthesis,
        comma,
        dot,
        close_parenthesis,
        operator_ne,
        operator_gt,
        operator_ge,
        operator_eq,
        operator_le,
        operator_lt,
        operator_and,
        operator_or,
        operator_not,
        operator_concat,
        value_identifier,
        value_true,
        value_false,
        value_number,
        value_string,
        open_bracket,
        close_bracket
    }

    private enum ePriority
    {
        none = 0,
        concat = 1,
        or = 2,
        and = 3,
        not = 4,
        equality = 5,
        plusminus = 6,
        muldiv = 7,
        exporoot = 8,
        percent = 9,
        unaryminus = 10
    }

    public class parserexception : Exception
    {
        internal parserexception(string str) : base(str)
        {
        }
    }

    private class tokenizer
    {
        private string mString;
        private int mLen;
        private int mPos;
        private char mCurChar;
        public int startpos;
        public eTokenType type;
        public System.Text.StringBuilder value = new System.Text.StringBuilder();
        private parser mParser;

        public tokenizer(parser Parser, string str)
        {
            mString = str;
            mLen = str.Length;
            mPos = 0;
            mParser = Parser;
            NextChar();   // start the machine
        }

        public void NextChar()
        {
            if (mPos < mLen)
            {
                mCurChar = mString[mPos];
                if (mCurChar == Strings.Chr(147) | mCurChar == Strings.Chr(148))
                    mCurChar = '"';
                mPos += 1;
            }
            else
                mCurChar = default(Char);
        }

        public bool IsOp()
        {
            return mCurChar == '+'
               | mCurChar == '-'
               | mCurChar == '%'
               | mCurChar == '/'
               | mCurChar == '('
               | mCurChar == ')'
               | mCurChar == '.';
        }

        public void NextToken()
        {
            startpos = mPos;
            value.Length = 0;
            type = eTokenType.none;
            do
            {
                switch (mCurChar)
                {
                    case default(Char):
                        {
                            type = eTokenType.end_of_formula;
                            break;
                        }

                    case object _ when '0' <= mCurChar && mCurChar <= '9':
                        {
                            ParseNumber();
                            break;
                        }

                    case '-':
                        {
                            NextChar();
                            type = eTokenType.operator_minus;
                            break;
                        }

                    case '+':
                        {
                            NextChar();
                            type = eTokenType.operator_plus;
                            break;
                        }

                    case '*':
                        {
                            NextChar();
                            type = eTokenType.operator_mul;
                            break;
                        }

                    case '/':
                        {
                            NextChar();
                            type = eTokenType.operator_div;
                            break;
                        }

                    case '^':
                        {
                            NextChar();
                            type = eTokenType.operator_exponent;
                            break;
                        }

                    case '%':
                        {
                            NextChar();
                            type = eTokenType.operator_percent;
                            break;
                        }

                    case '(':
                        {
                            NextChar();
                            type = eTokenType.open_parenthesis;
                            break;
                        }

                    case ')':
                        {
                            NextChar();
                            type = eTokenType.close_parenthesis;
                            break;
                        }

                    case '<':
                        {
                            NextChar();
                            if (mCurChar == '=')
                            {
                                NextChar();
                                type = eTokenType.operator_le;
                            }
                            else if (mCurChar == '>')
                            {
                                NextChar();
                                type = eTokenType.operator_ne;
                            }
                            else
                                type = eTokenType.operator_lt;
                            break;
                        }

                    case '>':
                        {
                            NextChar();
                            if (mCurChar == '=')
                            {
                                NextChar();
                                type = eTokenType.operator_ge;
                            }
                            else
                                type = eTokenType.operator_gt;
                            break;
                        }

                    case ',':
                        {
                            NextChar();
                            type = eTokenType.comma;
                            break;
                        }

                    case '=':
                        {
                            NextChar();
                            type = eTokenType.operator_eq;
                            break;
                        }

                    case '.':
                        {
                            NextChar();
                            type = eTokenType.dot;
                            break;
                        }

                    case '\'':
                    case '"':
                        {
                            ParseString(true);
                            type = eTokenType.value_string;
                            break;
                        }

                    case '&':
                        {
                            NextChar();
                            type = eTokenType.operator_concat;
                            break;
                        }

                    case '[':
                        {
                            NextChar();
                            type = eTokenType.open_bracket;
                            break;
                        }

                    case ']':
                        {
                            NextChar();
                            type = eTokenType.close_bracket;
                            break;
                        }

                    case object _ when Strings.Chr(0) <= mCurChar && mCurChar <= ' ':
                        {
                            break;
                        }

                    default:
                        {
                            ParseIdentifier();
                            break;
                        }
                }

                if (type != eTokenType.none)
                    break;
                NextChar();
            }
            while (true);
        }

        public void ParseNumber()
        {
            type = eTokenType.value_number;
            while (mCurChar >= '0' & mCurChar <= '9')
            {
                value.Append(mCurChar);
                NextChar();
            }
            if (mCurChar == '.')
            {
                value.Append(mCurChar);
                NextChar();
                while (mCurChar >= '0' & mCurChar <= '9')
                {
                    value.Append(mCurChar);
                    NextChar();
                }
            }
        }

        public void ParseIdentifier()
        {
            while ((mCurChar >= '0' & mCurChar <= '9') | (mCurChar >= 'a' & mCurChar <= 'z') | (mCurChar >= 'A' & mCurChar <= 'Z') | (mCurChar >= 'A' & mCurChar <= 'Z') | (mCurChar >= Strings.Chr(128)) | (mCurChar == '_'))
            {
                value.Append(mCurChar);
                NextChar();
            }
            switch (value.ToString())
            {
                case "and":
                    {
                        type = eTokenType.operator_and;
                        break;
                    }

                case "or":
                    {
                        type = eTokenType.operator_or;
                        break;
                    }

                case "not":
                    {
                        type = eTokenType.operator_not;
                        break;
                    }

                case "true":
                case "yes":
                    {
                        type = eTokenType.value_true;
                        break;
                    }

                case "false":
                case "no":
                    {
                        type = eTokenType.value_false;
                        break;
                    }

                default:
                    {
                        type = eTokenType.value_identifier;
                        break;
                    }
            }
        }

        public void ParseString(bool InQuote)
        {
            char OriginalChar = '0';
            if (InQuote)
            {
                OriginalChar = mCurChar;
                NextChar();
            }

            while (mCurChar != default(Char))
            {
                if (InQuote && mCurChar == OriginalChar)
                {
                    NextChar();
                    if (mCurChar == OriginalChar)
                        value.Append(mCurChar);
                    else
                        // End of String
                        return;
                }
                else if (mCurChar == '%')
                {
                    NextChar();
                    if (mCurChar == '[')
                    {
                        NextChar();
                        System.Text.StringBuilder SaveValue = value;
                        int SaveStartPos = startpos;
                        this.value = new System.Text.StringBuilder();
                        this.NextToken(); // restart the tokenizer for the subExpr
                        object subExpr;
                        try
                        {
                            subExpr = mParser.ParseExpr(0, ePriority.none);
                            if (subExpr == null)
                                this.value.Append("<nothing>");
                            else
                                this.value.Append(subExpr.ToString());
                        }
                        catch (Exception ex)
                        {
                            this.value.Append("<error " + ex.Message + ">");
                        }
                        SaveValue.Append(value.ToString());
                        value = SaveValue;
                        startpos = SaveStartPos;
                    }
                    else
                        value.Append('%');
                }
                else
                {
                    value.Append(mCurChar);
                    NextChar();
                }
            }
            if (InQuote)
                RaiseError("Incomplete string, missing " + OriginalChar + "; String started");
        }

        public void RaiseError(string msg, Exception ex = null)
        {
            if (ex is parserexception)
                msg += ". " + ex.Message;
            else
            {
                msg += " " + " at position " + startpos;
                if (ex != null)
                    msg += ". " + ex.Message;
            }
            throw new parserexception(msg);
        }

        public void RaiseUnexpectedToken(string msg = null)
        {
            if (Strings.Len(msg) == 0)
                msg = "";
            else
                msg += "; ";
            RaiseError(msg + "Unexpected " + type.ToString().Replace('_', ' ') + " : " + value.ToString());
        }

        public void RaiseWrongOperator(eTokenType tt, object ValueLeft, object valueRight, string msg = null)
        {
            if (Strings.Len(msg) > 0)
            {
                msg.Replace("[op]", tt.GetType().ToString());
                msg += ". ";
            }
            msg = "Cannot apply the operator " + tt.ToString();
            if (ValueLeft == null)
                msg += " on nothing";
            else
                msg += " on a " + ValueLeft.GetType().ToString();
            if (valueRight != null)
                msg += " and a " + valueRight.GetType().ToString();
            RaiseError(msg);
        }
    }

    private class parser
    {
        private tokenizer tokenizer;
        private Evaluator mEvaluator;
        // Private mEvalBinder As New EvalBinder
        private EvalFunctions mEvalFunctions = new EvalFunctions();

        public parser(Evaluator evaluator)
        {
            mEvaluator = evaluator;
        }

        internal object ParseExpr(object Acc, ePriority priority)
        {
            object ValueLeft = new object();
            object valueRight = new object();

            bool continueLoop = true;

            do
            {
                switch (tokenizer.type)
                {
                    case eTokenType.operator_minus:
                        {
                            // unary minus operator
                            tokenizer.NextToken();
                            ValueLeft = ParseExpr(0, ePriority.unaryminus);
                            if (ValueLeft is double)
                                ValueLeft = -(double)ValueLeft;
                            else
                                tokenizer.RaiseWrongOperator(eTokenType.operator_minus, ValueLeft, null, "You can use [op] only with numbers");

                            continueLoop = false;
                            break;
                        }
                    case eTokenType.operator_plus:
                        {
                            tokenizer.NextToken();
                            break;
                        }
                    case eTokenType.operator_not:
                        {
                            tokenizer.NextToken();
                            ValueLeft = ParseExpr(0, ePriority.not);
                            if (ValueLeft is bool)
                                ValueLeft = !(bool)ValueLeft;
                            else
                                tokenizer.RaiseWrongOperator(eTokenType.operator_not, ValueLeft, null, "You can use [op] only with boolean values");
                            break;
                        }
                    case eTokenType.value_identifier:
                        {
                            ValueLeft = InternalGetVariable();
                            continueLoop = false;
                            break;
                        }
                    case eTokenType.value_true:
                        {
                            ValueLeft = true;
                            tokenizer.NextToken();
                            continueLoop = false;
                            break;
                        }
                    case eTokenType.value_false:
                        {
                            ValueLeft = false;
                            tokenizer.NextToken();
                            continueLoop = false;
                            break;
                        }
                    case eTokenType.value_string:
                        {
                            ValueLeft = tokenizer.value.ToString();
                            tokenizer.NextToken();
                            continueLoop = false;
                            break;
                        }
                    case eTokenType.value_number:
                        {
                            ValueLeft = double.Parse(tokenizer.value.ToString());
                            tokenizer.NextToken();
                            continueLoop = false;
                            break;
                        }
                    case eTokenType.open_parenthesis:
                        {
                            tokenizer.NextToken();
                            ValueLeft = ParseExpr(0, ePriority.none);
                            if (tokenizer.type == eTokenType.close_parenthesis)
                            {
                                // good we eat the end parenthesis and continue ...
                                tokenizer.NextToken();
                                continueLoop = false;
                            }
                            else
                                tokenizer.RaiseUnexpectedToken("End parenthesis not found");
                            break;
                        }
                    default:
                        {
                            continueLoop = false;
                            break;
                        }
                }
            }
            while (continueLoop);

            continueLoop = true;

            do
            {
                eTokenType tt;
                tt = tokenizer.type;
                switch (tt)
                {
                    case eTokenType.end_of_formula:
                    {
                        return ValueLeft;
                    }
                    case eTokenType.value_number:
                    {
                        tokenizer.RaiseUnexpectedToken("Unexpected number without previous opterator");
                        return null;
                    }
                    case eTokenType.operator_plus:
                    {
                        if (priority < ePriority.plusminus)
                        {
                            tokenizer.NextToken();
                            valueRight = ParseExpr(ValueLeft, ePriority.plusminus);
                            if (ValueLeft is double & valueRight is double)
                                ValueLeft = System.Convert.ToDouble(ValueLeft) + System.Convert.ToDouble(valueRight);
                            else
                                ValueLeft = ValueLeft.ToString() + valueRight.ToString();
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    case eTokenType.operator_minus:
                    {
                        if (priority < ePriority.plusminus)
                        {
                            tokenizer.NextToken();
                            valueRight = ParseExpr(ValueLeft, ePriority.plusminus);
                            if (ValueLeft is double & valueRight is double)
                                ValueLeft = System.Convert.ToDouble(ValueLeft) - System.Convert.ToDouble(valueRight);
                            else
                                tokenizer.RaiseWrongOperator(tt, ValueLeft, valueRight, "You can use [op] only with numbers or dates");
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    case eTokenType.operator_exponent:
                    {
                        if (priority < ePriority.exporoot)
                        {
                            tokenizer.NextToken();
                            valueRight = ParseExpr(ValueLeft, ePriority.exporoot);
                            if (ValueLeft is double & valueRight is double)
                                ValueLeft = Math.Pow(System.Convert.ToDouble(ValueLeft), System.Convert.ToDouble(valueRight));
                            else
                                tokenizer.RaiseWrongOperator(tt, ValueLeft, valueRight, "You can use [op] only with numbers or dates");
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    case eTokenType.operator_concat:
                    {
                        if (priority < ePriority.concat)
                        {
                            tokenizer.NextToken();
                            valueRight = ParseExpr(ValueLeft, ePriority.concat);
                            ValueLeft = ValueLeft.ToString() + valueRight.ToString();
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    case eTokenType.operator_mul:
                    case eTokenType.operator_div:
                    {
                        if (priority < ePriority.muldiv)
                        {
                            tokenizer.NextToken();
                            valueRight = ParseExpr(ValueLeft, ePriority.muldiv);
                            if (ValueLeft is double & valueRight is double)
                            {
                                if (tt == eTokenType.operator_mul)
                                    ValueLeft = System.Convert.ToDouble(ValueLeft) * System.Convert.ToDouble(valueRight);
                                else
                                    ValueLeft = System.Convert.ToDouble(ValueLeft) / System.Convert.ToDouble(valueRight);
                            }
                            else
                                tokenizer.RaiseError("Cannot apply the operator * or / on a " + ValueLeft.GetType().Name + " and " + valueRight.GetType().Name + Constants.vbCrLf + "You can use - only with numbers");
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    case eTokenType.operator_percent:
                    {
                        if (priority < ePriority.percent)
                        {
                            tokenizer.NextToken();
                            if (ValueLeft is double & Acc is double)
                                ValueLeft = System.Convert.ToDouble(Acc) * System.Convert.ToDouble(ValueLeft) / 100.0;
                            else
                            {
                                string ValueLeftString;
                                if (ValueLeft == null)
                                    ValueLeftString = "nothing";
                                else
                                    ValueLeftString = ValueLeft.GetType().ToString();
                                tokenizer.RaiseError("Cannot apply the operator + or - on a " + ValueLeftString + " and " + valueRight.GetType().Name + Constants.vbCrLf + "You can use % only with numbers. For example 150 + 20.5% ");
                            }
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    case eTokenType.operator_or:
                    {
                        if (priority < ePriority.or)
                        {
                            tokenizer.NextToken();
                            valueRight = ParseExpr(ValueLeft, ePriority.or);
                            if (ValueLeft is bool & valueRight is bool)
                                ValueLeft = System.Convert.ToBoolean(ValueLeft) | System.Convert.ToBoolean(valueRight);
                            else
                                tokenizer.RaiseError("Cannot apply the operator OR on a " + ValueLeft.GetType().Name + " and " + valueRight.GetType().Name + Constants.vbCrLf + "You can use OR only with boolean values");
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    case eTokenType.operator_and:
                    {
                        if (priority < ePriority.and)
                        {
                            tokenizer.NextToken();
                            valueRight = ParseExpr(ValueLeft, ePriority.and);
                            if (ValueLeft is bool & valueRight is bool)
                                ValueLeft = System.Convert.ToBoolean(ValueLeft) & System.Convert.ToBoolean(valueRight);
                            else
                                tokenizer.RaiseWrongOperator(tt, ValueLeft, valueRight, "You can use [op] only with boolean values");
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    case eTokenType.operator_ne:
                    case eTokenType.operator_gt:
                    case eTokenType.operator_ge:
                    case eTokenType.operator_eq:
                    case eTokenType.operator_le:
                    case eTokenType.operator_lt:
                    {
                        if (priority < ePriority.equality)
                        {
                            tt = tokenizer.type;
                            tokenizer.NextToken();
                            valueRight = ParseExpr(ValueLeft, ePriority.equality);
                            if (ValueLeft is IComparable && ValueLeft.GetType().Equals(valueRight.GetType()))
                            {
                                IComparable left = (IComparable)ValueLeft;
                                int cmp = left.CompareTo(valueRight);
                                switch (tt)
                                {
                                    case eTokenType.operator_ne:
                                        {
                                            ValueLeft = (cmp != 0);
                                            break;
                                        }
                                    case eTokenType.operator_lt:
                                        {
                                            ValueLeft = (cmp < 0);
                                            break;
                                        }
                                    case eTokenType.operator_le:
                                        {
                                            ValueLeft = (cmp <= 0);
                                            break;
                                        }
                                    case eTokenType.operator_eq:
                                        {
                                            ValueLeft = (cmp == 0);
                                            break;
                                        }
                                    case eTokenType.operator_ge:
                                        {
                                            ValueLeft = (cmp >= 0);
                                            break;
                                        }
                                    case eTokenType.operator_gt:
                                        {
                                            ValueLeft = (cmp > 0);
                                            break;
                                        }
                                }
                            }
                            else
                                tokenizer.RaiseWrongOperator(tt, ValueLeft, valueRight);
                        }
                        else
                            continueLoop = false;
                        break;
                    }
                    default:
                    {
                        continueLoop = false;
                        break;
                    }
                }
            }
            while (continueLoop);

            return ValueLeft;
        }

        private object InternalGetVariable()
        {
            // first check functions
            List<Object> parameters = new List<object>();
            // Dim types As New ArrayList
            object valueleft = new object();
            object CurrentObject = new object();

            bool continueLoop = true;
            bool continueLoop2 = true;
            
            do
            {
                string func = tokenizer.value.ToString();
                tokenizer.NextToken();
                parameters.Clear();
                // types.Clear()
                if (tokenizer.type == eTokenType.open_parenthesis)
                {
                    tokenizer.NextToken();
                    do
                    {
                        if (tokenizer.type == eTokenType.close_parenthesis)
                        {
                            // good we eat the end parenthesis and continue ...
                            tokenizer.NextToken();
                            continueLoop2 = false;
                            break;
                        }

                        valueleft = ParseExpr(0, ePriority.none);
                        parameters.Add(valueleft);
                        // If valueleft Is Nothing Then
                        // types.Add(Nothing)
                        // Else
                        // types.Add(valueleft.GetType())
                        // End If
                        if (tokenizer.type == eTokenType.close_parenthesis)
                        {
                            // good we eat the end parenthesis and continue ...
                            tokenizer.NextToken();
                            continueLoop2 = false;
                            break;
                        }
                        else if (tokenizer.type == eTokenType.comma)
                            tokenizer.NextToken();
                        else
                            tokenizer.RaiseUnexpectedToken("End parenthesis not found");
                    }
                    while (continueLoop2);
                }

                System.Reflection.MethodInfo mi = null;
                System.Reflection.PropertyInfo pi = null;

                CurrentObject = mEvalFunctions;

                if (CurrentObject == null)
                {
                    mi = CurrentObject.GetType().GetMethod(func);
                    // Reflection.BindingFlags.Public _
                    // Or Reflection.BindingFlags.Instance _
                    // Or Reflection.BindingFlags.IgnoreCase, _
                    // mEvalBinder, _
                    // DirectCast(types.ToArray(GetType(Type)), Type()), _
                    // Nothing)
                    if (mi == null && mEvaluator.mExtraFunctions != null)
                    {
                        CurrentObject = mEvaluator.mExtraFunctions;
                        mi = CurrentObject.GetType().GetMethod(func);
                    }
                }
                else
                {
                    mi = CurrentObject.GetType().GetMethod(func, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (mi == null)
                        pi = CurrentObject.GetType().GetProperty(func, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                }

                if (mi != null)
                {
                    try
                    {
                        int idx = 0;
                        object param;
                        foreach (System.Reflection.ParameterInfo pri in mi.GetParameters())
                        {
                            if (idx < parameters.Count)
                                param = parameters[idx];
                            else
                            {
                                param = pri.DefaultValue;
                                parameters.Add(param);
                            }

                            idx += 1;
                        }

                        valueleft = mi.Invoke(CurrentObject, System.Reflection.BindingFlags.Default, null/* TODO Change to default(_) if this is not a reference type */, (object[])parameters.ToArray(), null/* TODO Change to default(_) if this is not a reference type */);
                    }
                    catch (Exception ex)
                    {
                        tokenizer.RaiseError("Error while running function " + func, ex);
                    }
                }
                else if (pi != null)
                {
                    try
                    {
                        int idx = 0;
                        object param;
                        foreach (System.Reflection.ParameterInfo pri in pi.GetIndexParameters())
                        {
                            if (idx < parameters.Count)
                                param = parameters[idx];
                            else
                            {
                                param = pri.DefaultValue;
                                parameters.Add(param);
                            }

                            idx += 1;
                        }

                        valueleft = pi.GetValue(CurrentObject, System.Reflection.BindingFlags.Default, null/* TODO Change to default(_) if this is not a reference type */, (object[])parameters.ToArray(), null/* TODO Change to default(_) if this is not a reference type */);
                    }
                    catch (Exception ex)
                    {
                        tokenizer.RaiseError("Error while running function " + func, ex);
                    }
                }
                else if (parameters.Count == 0)
                {
                    // then raise event
                    valueleft = null;
                    try
                    {
                        mEvaluator.RaiseGetVariable(func, ref valueleft);
                    }
                    catch (Exception ex)
                    {
                        tokenizer.RaiseError("Error while raising event get variable" + func, ex);
                    }

                    if (valueleft == null)
                        tokenizer.RaiseError("Unknown variable " + func);
                }
                else
                    tokenizer.RaiseError("Unknown function " + func);
                if (tokenizer.type == eTokenType.dot)
                {
                    // continue with the current object...
                    tokenizer.NextToken();
                    CurrentObject = valueleft;
                }
                else
                {
                    continueLoop = false;
                    break;
                }
            }
            while (continueLoop)// eat the parenthesis
;
            return valueleft;
        }

        public object Eval(string str)
        {
            if (Strings.Len(str) > 0)
            {
                tokenizer = new tokenizer(this, str);
                tokenizer.NextToken();
                object res = ParseExpr(null, ePriority.none);
                if (tokenizer.type == eTokenType.end_of_formula)
                    return res;
                else
                    tokenizer.RaiseUnexpectedToken();
            }
            return null;
        }

        public string EvalString(string str)
        {
            if (Strings.Len(str) > 0)
            {
                tokenizer = new tokenizer(this, str);
                tokenizer.ParseString(false);
                return tokenizer.value.ToString();
            }
            else
                return string.Empty;
        }
    }

    public object ExtraFunctions
    {
        get
        {
            return mExtraFunctions;
        }
        set
        {
            mExtraFunctions = value;
        }
    }

    public double EvalDouble(ref string formula)
    {
        object res = Eval(formula);
        if (res is double)
            return System.Convert.ToDouble(res);
        else
            throw new parserexception("The result is not a number : " + res.ToString());
    }

    public object Eval(string str)
    {
        return mParser.Eval(str);
    }

    public string EvalString(string str)
    {
        return mParser.EvalString(str);
    }

    private void RaiseGetVariable(string name, ref object value)
    {
        GetVariable?.Invoke(name, ref value);
        if (value is float | value is int)
            value = (double)value;
    }

    public event GetVariableEventHandler GetVariable;

    public delegate void GetVariableEventHandler(string name, ref object value);
}
