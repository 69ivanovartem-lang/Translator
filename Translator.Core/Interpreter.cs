using System;
using System.Collections.Generic;

namespace Translator.Core
{
    public class Interpreter
    {
        private Dictionary<string, bool> variables = new Dictionary<string, bool>();
        private List<string> output = new List<string>();

        public string Execute(string code)
        {
            output.Clear();
            variables.Clear();

            try
            {
                string[] lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Объявление переменных
                if (lines.Length < 1) return "Ошибка: нет объявления переменных";

                string varLine = lines[0].Trim();
                if (varLine.StartsWith("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    string varPart = varLine.Substring(7).Trim();
                    string[] varNames = varPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var name in varNames)
                    {
                        string varName = name.Trim();
                        if (!variables.ContainsKey(varName))
                            variables[varName] = false;
                    }
                }

                // Поиск Begin и End
                int beginIndex = -1, endIndex = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Trim().Equals("Begin", StringComparison.OrdinalIgnoreCase))
                        beginIndex = i;
                    if (lines[i].Trim().Equals("End", StringComparison.OrdinalIgnoreCase))
                        endIndex = i;
                }

                if (beginIndex == -1 || endIndex == -1)
                    return "Ошибка: не найдены Begin/End";

                // Выполнение присваиваний
                for (int i = beginIndex + 1; i < endIndex; i++)
                {
                    string line = lines[i].Trim();
                    if (!string.IsNullOrEmpty(line))
                        ExecuteAssignment(line);
                }

                // Выполнение Print
                for (int i = endIndex + 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.StartsWith("Print", StringComparison.OrdinalIgnoreCase))
                    {
                        string varName = line.Substring(5).Trim();
                        if (variables.ContainsKey(varName))
                        {
                            output.Add(variables[varName] ? "True" : "False");
                        }
                    }
                }

                return output.Count > 0 ? string.Join("\n", output) : "Нет вывода";
            }
            catch (Exception ex)
            {
                return $"Ошибка выполнения: {ex.Message}";
            }
        }

        private void ExecuteAssignment(string line)
        {
            string[] parts = line.Split(new[] { ":=" }, StringSplitOptions.None);
            if (parts.Length != 2) return;

            string varName = parts[0].Trim();
            string expression = parts[1].Trim();

            bool result = EvaluateExpression(expression);

            if (variables.ContainsKey(varName))
                variables[varName] = result;
        }

        private bool EvaluateExpression(string expr)
        {
            expr = expr.Trim();

            // .NOT.
            if (expr.StartsWith(".NOT.", StringComparison.OrdinalIgnoreCase))
            {
                string operand = expr.Substring(5).Trim();
                return !EvaluateExpression(operand);
            }

            // Скобки
            if (expr.StartsWith("(") && expr.EndsWith(")"))
            {
                return EvaluateExpression(expr.Substring(1, expr.Length - 2));
            }

            // Бинарные операторы
            string[] operators = { ".XOR.", ".AND.", ".OR." };
            foreach (string op in operators)
            {
                if (expr.Contains(op))
                {
                    string[] parts = expr.Split(new[] { op }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        bool left = EvaluateExpression(parts[0].Trim());
                        bool right = EvaluateExpression(parts[1].Trim());

                        switch (op)
                        {
                            case ".XOR.": return left != right;
                            case ".AND.": return left && right;
                            case ".OR.": return left || right;
                        }
                    }
                }
            }

            // Константы и переменные
            if (expr.Equals("True", StringComparison.OrdinalIgnoreCase))
                return true;
            if (expr.Equals("False", StringComparison.OrdinalIgnoreCase))
                return false;

            if (variables.ContainsKey(expr))
                return variables[expr];

            return false;
        }
    }
}