using System;
using System.Collections.Generic;
using System.Globalization;

public class ExpressionEvaluator
{
    public static double Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression cannot be empty");
        }

        List<Token> tokens = ParseExpression(expression);

        if (tokens.Count == 0)
        {
            throw new ArgumentException("Invalid expression");
        }

        tokens = EvaluateOperations(tokens, new[] { '÷', '×' });
        tokens = EvaluateOperations(tokens, new[] { '+', '-' });

        if (tokens.Count != 1 || tokens[0].Type != TokenType.Number)
        {
            throw new ArgumentException("Invalid expression format");
        }

        return tokens[0].Value;
    }

    private static List<Token> ParseExpression(string expression)
    {
        List<Token> tokens = new List<Token>();
        string currentNumber = "";

        for (int i = 0; i < expression.Length; i++)
        {
            char c = expression[i];

            if (char.IsDigit(c) || c == '.')
            {
                currentNumber += c;
            }
            else if (IsOperator(c))
            {
                if (!string.IsNullOrEmpty(currentNumber))
                {
                    if (double.TryParse(currentNumber, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    {
                        tokens.Add(new Token(TokenType.Number, value));
                        currentNumber = "";
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid number: {currentNumber}");
                    }
                }

                if (c == '-' && (tokens.Count == 0 || tokens[tokens.Count - 1].Type == TokenType.Operator))
                {
                    currentNumber = "-";
                }
                else
                {
                    tokens.Add(new Token(TokenType.Operator, c));
                }
            }
            else if (char.IsWhiteSpace(c))
            {
                continue;
            }
            else
            {
                throw new ArgumentException($"Invalid character in expression: {c}");
            }
        }

        if (!string.IsNullOrEmpty(currentNumber))
        {
            if (double.TryParse(currentNumber, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                tokens.Add(new Token(TokenType.Number, value));
            }
            else
            {
                throw new ArgumentException($"Invalid number: {currentNumber}");
            }
        }

        return tokens;
    }

    private static List<Token> EvaluateOperations(List<Token> tokens, char[] operators)
    {
        if (tokens.Count < 3)
        {
            return tokens;
        }

        List<Token> result = new List<Token>();
        int i = 0;

        while (i < tokens.Count)
        {
            if (i + 2 < tokens.Count &&
                tokens[i].Type == TokenType.Number &&
                tokens[i + 1].Type == TokenType.Operator &&
                tokens[i + 2].Type == TokenType.Number &&
                Array.IndexOf(operators, (char)tokens[i + 1].Value) >= 0)
            {
                double left = tokens[i].Value;
                char op = (char)tokens[i + 1].Value;
                double right = tokens[i + 2].Value;

                double resultValue = PerformOperation(left, op, right);
                result.Add(new Token(TokenType.Number, resultValue));

                i += 3;
            }
            else
            {
                result.Add(tokens[i]);
                i++;
            }
        }

        if (result.Count < tokens.Count)
        {
            return EvaluateOperations(result, operators);
        }

        return result;
    }

    private static double PerformOperation(double left, char op, double right)
    {
        switch (op)
        {
            case '+':
                return left + right;
            case '-':
                return left - right;
            case '×':
                return left * right;
            case '÷':
                if (Math.Abs(right) < double.Epsilon)
                {
                    throw new DivideByZeroException("Division by zero is not allowed");
                }
                return left / right;
            default:
                throw new ArgumentException($"Unknown operator: {op}");
        }
    }

    private static bool IsOperator(char c)
    {
        return c == '+' || c == '-' || c == '×' || c == '÷';
    }

    private class Token
    {
        public TokenType Type { get; }
        public double Value { get; }

        public Token(TokenType type, double value)
        {
            Type = type;
            Value = value;
        }
    }

    private enum TokenType
    {
        Number,
        Operator
    }
}
