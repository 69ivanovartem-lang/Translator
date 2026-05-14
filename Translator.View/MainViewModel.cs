using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Windows;
using Translator.Core;
using Translator.View.ViewModel.Core;

namespace Translator.View.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        const string SourceFileName = "code";
        const string ProgramFileName = "compile";

        private RelayCommand? openFileCommand = null;
        public RelayCommand OpenFileCommand
        {
            get
            {
                return openFileCommand ??
                  (openFileCommand = new RelayCommand(obj =>
                  {
                      OpenFile();
                  }));
            }
        }

        private void OpenFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (Path.GetExtension(openFileDialog.FileName) != ".txt")
                {
                    MessageBox.Show("Пожалуйста, выберите текстовый файл (.txt)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                InputText = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private RelayCommand? saveFileCommand = null;
        public RelayCommand SaveFileCommand
        {
            get
            {
                return saveFileCommand ??
                  (saveFileCommand = new RelayCommand(obj =>
                  {
                      SaveFile();
                  }));
            }
        }

        private void SaveFile()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, InputText);
            }
        }

        private RelayCommand? compileAndRunCommand = null;
        public RelayCommand CompileAndRunCommand
        {
            get
            {
                return compileAndRunCommand ??
                  (compileAndRunCommand = new RelayCommand(obj =>
                  {
                      CompileAndRun();
                  }));
            }
        }

        private void CompileAndRun()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sourceFilePath = Path.Combine(baseDirectory, SourceFileName + ".txt");
            string compiledFilePath = Path.Combine(baseDirectory, ProgramFileName + ".asm");

            // Сохраняем исходный код
            File.WriteAllText(sourceFilePath, InputText);

            try
            {
                // Компилируем
                var syntaxAnalyzer = new SyntaxAnalyzer();
                syntaxAnalyzer.Compile(InputText);

                // Получаем сгенерированный код
                var code = string.Join("\n", CodeGenerator.GetGeneratedCode());
                OutputText = code;

                // Сохраняем ASM файл
                File.WriteAllText(compiledFilePath, code);

                // Показываем сообщение об успехе
                MessageBox.Show(
                    $"Компиляция успешно завершена!\n\n" +
                    $"Сгенерированный ассемблерный код сохранен в:\n{compiledFilePath}\n\n" +
                    $"Код содержит {CodeGenerator.GetGeneratedCode().Length} строк.\n\n" +
                    $"Для запуска в DOSBox:\n" +
                    $"1. Установите DOSBox\n" +
                    $"2. Скопируйте файл {ProgramFileName}.asm в папку с MASM\n" +
                    $"3. Выполните: MASM {ProgramFileName}.asm; LINK {ProgramFileName}.obj; {ProgramFileName}.exe",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                OutputText = $"Ошибка компиляции: {ex.Message}";
                MessageBox.Show(ex.Message, "Ошибка компиляции", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string _inputText = string.Empty;
        private string _outputText = string.Empty;

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged(nameof(OutputText));
            }
        }
    }
}