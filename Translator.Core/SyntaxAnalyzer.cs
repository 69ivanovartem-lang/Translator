namespace Translator.Core
{
    /// <summary>
    /// Класс, ответственный за синтаксический анализ и компиляцию исходного кода.
    /// </summary>
    public class SyntaxAnalyzer
    {
        private NameTable nameTable = new NameTable();

        /// <summary>
        /// Компилирует исходный код.
        /// </summary>
        public void Compile(string code)
        {
            LexicalAnalyzer.Initialize(code);
            CodeGenerator.Initialize();
            CodeGenerator.DeclareDataSegment();

            LexicalAnalyzer.ParseNextLexem();

            // <Программа> ::= <Объявление переменных> <Описание вычислений> <Оператор печати>
            ParseVariableDeclaration();      // <Объявление переменных>
            CodeGenerator.DeclareVariables(nameTable);
            CodeGenerator.DeclareStackAndCodeSegments();

            ParseComputationDescription();   // <Описание вычислений>
            ParsePrintStatement();           // <Оператор печати>

            CodeGenerator.DeclareMainProcedureEnd();
            CodeGenerator.DeclarePrintProcedure();
            CodeGenerator.DeclareEndOfCode();
        }

        /// <summary>
        /// <Объявление переменных> ::= "Boolean" <Список переменных>
        /// </summary>
        private void ParseVariableDeclaration()
        {
            CheckLexem(Lexems.Boolean);
            ParseVariableList();
        }

        /// <summary>
        /// <Список переменных> ::= <Идент> | <Идент> "," <Список переменных>
        /// </summary>
        private void ParseVariableList()
        {
            List<string> variables = new List<string>();

            do
            {
                if (LexicalAnalyzer.CurrentLexem == Lexems.Name)
                {
                    string varName = LexicalAnalyzer.CurrentName ?? string.Empty;
                    variables.Add(varName);
                    LexicalAnalyzer.ParseNextLexem();

                    if (LexicalAnalyzer.CurrentLexem == Lexems.Comma)
                    {
                        LexicalAnalyzer.ParseNextLexem();
                        continue;
                    }
                    break;
                }
                else
                {
                    Error();
                }
            } while (true);

            foreach (var varName in variables)
            {
                nameTable.AddIdentifier(varName, tCat.Var, tType.Bool);
            }
        }

        /// <summary>
        /// <Описание вычислений> ::= "Begin" <Список присваиваний> "End"
        /// </summary>
        private void ParseComputationDescription()
        {
            CheckLexem(Lexems.Begin);
            ParseAssignmentList();
            CheckLexem(Lexems.End);
            CheckLexem(Lexems.Semi);
        }

        /// <summary>
        /// <Список присваиваний> ::= <Присваивание> | <Присваивание> <Список присваиваний>
        /// </summary>
        private void ParseAssignmentList()
        {
            ParseAssignment();

            while (LexicalAnalyzer.CurrentLexem == Lexems.Name ||
                   LexicalAnalyzer.CurrentLexem == Lexems.Semi)
            {
                if (LexicalAnalyzer.CurrentLexem == Lexems.Semi)
                    LexicalAnalyzer.ParseNextLexem();

                if (LexicalAnalyzer.CurrentLexem == Lexems.Name)
                    ParseAssignment();
                else
                    break;
            }
        }

        /// <summary>
        /// <Присваивание> ::= <Идент> ":=" <Выражение>
        /// </summary>
        private void ParseAssignment()
        {
            string varName = LexicalAnalyzer.CurrentName ?? string.Empty;
            Identifier var = nameTable.FindByName(varName);

            if (var.Equals(default(Identifier)))
                Error();

            LexicalAnalyzer.ParseNextLexem();
            CheckLexem(Lexems.Assign);

            tType exprType = ParseExpression();

            if (exprType != tType.Bool)
                throw new Exception($"Ошибка: Присваивание логической переменной {varName} нелогического выражения");

            CodeGenerator.AddInstruction("pop ax");
            CodeGenerator.AddInstruction($"mov {varName}, ax");
        }

        /// <summary>
        /// <Выражение> ::= <Ун.оп.> <Подвыражение> | <Подвыражение>
        /// </summary>
        private tType ParseExpression()
        {
            if (LexicalAnalyzer.CurrentLexem == Lexems.DotNot)
            {
                LexicalAnalyzer.ParseNextLexem();
                tType type = ParseSubexpression();

                if (type != tType.Bool)
                    Error();

                CodeGenerator.AddNegationInstruction();
                return tType.Bool;
            }
            else
            {
                return ParseSubexpression();
            }
        }

        /// <summary>
        /// <Подвыражение> ::= "(" <Выражение> ")" | <Операнд> | <Подвыражение> <Бин.оп.> <Подвыражение>
        /// </summary>
        private tType ParseSubexpression()
        {
            // Обработка скобок: "(" <Выражение> ")"
            if (LexicalAnalyzer.CurrentLexem == Lexems.LeftBracket)
            {
                LexicalAnalyzer.ParseNextLexem();
                tType type = ParseExpression();
                CheckLexem(Lexems.RightBracket);
                return type;
            }

            // Левый операнд
            tType leftType = ParseOperand();

            // Обработка бинарных операций
            while (LexicalAnalyzer.CurrentLexem == Lexems.Conjunction ||
                   LexicalAnalyzer.CurrentLexem == Lexems.Disjunction ||
                   LexicalAnalyzer.CurrentLexem == Lexems.Xor)
            {
                Lexems op = LexicalAnalyzer.CurrentLexem;
                LexicalAnalyzer.ParseNextLexem();
                tType rightType = ParseOperand();

                if (leftType != tType.Bool || rightType != tType.Bool)
                    Error();

                switch (op)
                {
                    case Lexems.Conjunction:
                        CodeGenerator.AddConjunctionInstruction();
                        break;
                    case Lexems.Disjunction:
                        CodeGenerator.AddDisjunctionInstruction();
                        break;
                    case Lexems.Xor:
                        CodeGenerator.AddXorInstruction();
                        break;
                }

                leftType = tType.Bool;
            }

            return leftType;
        }

        /// <summary>
        /// <Операнд> ::= <Идент> | <Const>
        /// </summary>
        private tType ParseOperand()
        {
            if (LexicalAnalyzer.CurrentLexem == Lexems.Name)
            {
                string varName = LexicalAnalyzer.CurrentName ?? string.Empty;
                Identifier id = nameTable.FindByName(varName);
                if (id.Equals(default(Identifier)))
                    Error();

                CodeGenerator.AddExtractValueInstruction(varName);
                LexicalAnalyzer.ParseNextLexem();
                return id.Type;
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.True)
            {
                CodeGenerator.AddExtractTrueInstruction();
                LexicalAnalyzer.ParseNextLexem();
                return tType.Bool;
            }
            else if (LexicalAnalyzer.CurrentLexem == Lexems.False)
            {
                CodeGenerator.AddExtractFalseInstruction();
                LexicalAnalyzer.ParseNextLexem();
                return tType.Bool;
            }
            else
            {
                Error();
                return tType.None;
            }
        }

        /// <summary>
        /// <Оператор печати> ::= "Print" <Идент>
        /// </summary>
        private void ParsePrintStatement()
        {
            CheckLexem(Lexems.Print);

            if (LexicalAnalyzer.CurrentLexem == Lexems.Name)
            {
                string varName = LexicalAnalyzer.CurrentName ?? string.Empty;
                Identifier var = nameTable.FindByName(varName);

                if (var.Equals(default(Identifier)))
                    Error();

                CodeGenerator.AddInstruction($"mov ax, {varName}");
                CodeGenerator.AddInstruction("push ax");
                CodeGenerator.AddInstruction("CALL PRINT");
                CodeGenerator.AddInstruction("pop ax");

                LexicalAnalyzer.ParseNextLexem();
            }
            else
            {
                Error();
            }
        }

        /// <summary>
        /// Проверяет, совпадает ли текущая лексема с ожидаемой.
        /// </summary>
        private void CheckLexem(Lexems expectedLexem)
        {
            if (LexicalAnalyzer.CurrentLexem != expectedLexem)
            {
                Error();
            }
            LexicalAnalyzer.ParseNextLexem();
        }

        /// <summary>
        /// Обрабатывает ошибки в процессе синтаксического анализа.
        /// </summary>
        private void Error()
        {
            throw new Exception(
                $"Ошибка в строке {Reader.LineNumber}, позиция {Reader.CharacterPositionInLine}: " +
                $"Неверная лексема: {LexicalAnalyzer.CurrentLexem}");
        }
    }
}