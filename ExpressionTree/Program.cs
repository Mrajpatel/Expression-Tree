using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ExpressionTree
{
    class Program
    {
        static void Main(string[] args)
        {
            bool varified = true;
            string varifiedString = "";

            while (varified)
            {
                Console.WriteLine("Enter Reverse Polish Notation expression: ");
                string enterString = Console.ReadLine();

                //checking if string has any invalid character
                int errorCounter = Regex.Matches(enterString, @"[a-zA-Z]").Count;

                try
                {
                    if (errorCounter == 0 && enterString.Trim() != "")
                    {
                        String[] splitValues = enterString.Split(' ');

                        //check if first two digits are numbers in string
                        var isNumeric_0 = int.TryParse(splitValues[0], out int n0);
                        var isNumeric_1 = int.TryParse(splitValues[1], out int n1);

                        if (isNumeric_0 && isNumeric_1)
                        {
                            //checking if the last character in the string is operator
                            var isOperator = Regex.Matches(splitValues[splitValues.Length - 1], @"^(\+|-|\*|/)$").Count;

                            //counting numbers and operators in string 
                            if (isOperator != 0)
                            {
                                int countNumbers = 0;
                                int countOperators = 0;

                                foreach (var a in splitValues)
                                {
                                    if (int.TryParse(a, out int b))
                                    {
                                        countNumbers++;
                                    }
                                    else
                                    {
                                        countOperators++;
                                    }
                                }

                                if ((countNumbers - 1) == countOperators)
                                {
                                    varified = false;
                                    Console.WriteLine("---Enter String is VALID for Reverse Polish Notation---");
                                    varifiedString = enterString; 
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("---Enter String is INVALID for Reverse Polish Notation---");
                                }
                            }
                            else
                            {
                                Console.WriteLine("---Enter String is INVALID for Reverse Polish Notation---");
                            }
                        }
                        else
                        {
                            Console.WriteLine("---Enter String is INVALID for Reverse Polish Notation---");
                        }

                    }
                    else
                    {
                        Console.WriteLine("---Enter String is INVALID for Reverse Polish Notation---");
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    Console.WriteLine("---Enter String is INVALID for Reverse Polish Notation---");
                }
            }

            if (!varified)
            {
                string notation = varifiedString;
                string[] notarionSplit = notation.Split(" ");

                Stack listConstant = new Stack();

                Expression<Func<double, double, double>> expression = (x, y) => x + y;

                foreach (var i in notarionSplit)
                {
                    if (Double.TryParse(i, out double x))
                    {
                        listConstant.Push(x);
                    }
                    else
                    {
                        switch (i)
                        {
                            case "*":
                                var expressionMultiply = (Expression<Func<double, double, double>>)(new MyExpressionVisitor(ExpressionType.Multiply)).Visit(expression);
                                Func<double, double, double> result = expressionMultiply.Compile();                             
                                listConstant = addStack(listConstant, expressionMultiply);
                                break;
                            case "-":
                                var expressionSub = (Expression<Func<double, double, double>>)(new MyExpressionVisitor(ExpressionType.Subtract)).Visit(expression);
                                listConstant = addStack(listConstant, expressionSub);
                                break;
                            case "+":
                                var expressionAdd = (Expression<Func<double, double, double>>)(new MyExpressionVisitor(ExpressionType.Add)).Visit(expression);
                                listConstant = addStack(listConstant, expressionAdd);
                                break;
                            case "/":
                                var expressionDivide = (Expression<Func<double, double, double>>)(new MyExpressionVisitor(ExpressionType.Divide)).Visit(expression);
                                listConstant = addStack(listConstant, expressionDivide);
                                break;
                        }
                    }
                }
                foreach (var j in listConstant)
                {
                    Console.WriteLine(j);
                }
            }
        }

        public static Stack addStack(Stack listConstant, Expression<Func<double, double, double>> otherExpression)
        {
            Func<double, double, double> result = otherExpression.Compile();
            double rhs = (Double)listConstant.Pop();
            double lhs = (Double)listConstant.Pop();
            listConstant.Push((result(lhs, rhs)));
            return listConstant;
        }


        public class MyExpressionVisitor : ExpressionVisitor
        {
            public MyExpressionVisitor() { }

            public MyExpressionVisitor(ExpressionType a)
            {
                this.Operator = a;
            }

            public ExpressionType Operator { get; set; }

            public override Expression Visit(Expression node)
            {
                //Console.WriteLine(node.NodeType); 
                // apply a switch case to the node of the expression 
                switch (node.NodeType)
                {
                    // if the expression is +,-,* or / we want to visit the binary expression 
                    case ExpressionType.Add:
                    case ExpressionType.Subtract:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                        return this.VisitBinary((BinaryExpression)node);
                    // if the expression is constant visit the constant expression 
                    case ExpressionType.Constant:
                        return this.VisitConstant((ConstantExpression)node);
                    default:
                        return base.Visit(node);
                }
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                // apply a switch case to the node type of the binary expression 
                switch (this.Operator)
                {
                    case ExpressionType.Multiply:
                        return Expression.MakeBinary(ExpressionType.Multiply, this.Visit(node.Left), this.Visit(node.Right));
                    case ExpressionType.Subtract:
                        return Expression.MakeBinary(ExpressionType.Subtract, this.Visit(node.Left), this.Visit(node.Right));
                    case ExpressionType.Add:
                        return Expression.MakeBinary(ExpressionType.Add, this.Visit(node.Left), this.Visit(node.Right));
                    case ExpressionType.Divide:
                        return Expression.MakeBinary(ExpressionType.Divide, this.Visit(node.Left), this.Visit(node.Right));
                }

                // return all other types of expressions normally as we do not want to change any other type of expression in our expression tree 
                return base.VisitBinary(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                return base.VisitConstant(node);
            }
        }
    }
}

