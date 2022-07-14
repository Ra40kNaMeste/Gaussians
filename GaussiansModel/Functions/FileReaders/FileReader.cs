using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GaussiansModel.Extension;

namespace GaussiansModel.Functions
{
    /// <summary>
    /// Менеджер классов по чтению графика из файла
    /// </summary>


    #region BaseFileReader

    /// <summary>
    /// Аттрибут, который необходимо давать всем классам - реализаторам IFileReader
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FileReaderAttribute : Attribute
    {
    }

    public interface IFileReader
    {
        public PointGraph ReadFile(Stream file);
    }

    /// <summary>
    /// Реализация базового класса для чтения - одиночки
    /// </summary>
    /// <typeparam name="T">Тип наследуемого класса</typeparam>
    public abstract class FileReaderBase<T> : NodeFunctionBase<T>, IFileReader where T : FileReaderBase<T>
    {
        public FileReaderBase()
        {
            Inputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.InputSource, typeof(StreamData), new StreamData(null, FileMode.Open))
            };
            Outputs = new List<FunctionParameter>()
            {
                new FunctionParameter(Properties.Resources.OutputGraph, typeof(PointGraph), new PointGraph())
            };
        }

        public override void Invoke()
        {
            SetOutputParameter(Properties.Resources.OutputGraph, ReadFile(((StreamData)FindInputParameter(Properties.Resources.InputSource).Value).Stream));
        }

        public abstract PointGraph ReadFile(Stream file);
    }
    #endregion//BaseFileReader

    #region RealizationsFileReader
    /// <summary>
    /// Некоторые скрипты
    /// </summary>
    internal static class ReaderSkripts
    {
        /// <summary>
        /// Добавляет лишь те строки, которые удовлетворяют условию
        /// </summary>
        /// <param name="file">Считывемый файл</param>
        /// <param name="canAddLineFunc">Функция условия добавления стоки</param>
        /// <returns></returns>
        public static IEnumerable<string> BuildStringArrayByFunction(Stream file, Func<string, bool> canAddLineFunc)
        {
            List<string> res = new();
            using (StreamReader sr = new(file))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                    if (canAddLineFunc(line))
                        res.Add(line);
            }
            return res;
        }
    }

    [FileReader]
    public class ReaderPRN : FileReaderBase<ReaderPRN>
    {
        public ReaderPRN() { }
        Regex regex = new(@"^\s*-?\d+([.|,]\d*)?([e|E]-\d*)?\s*-?\d+([.|,]\d*)?([e|E]-\d*)?\s*$");
        public override PointGraph ReadFile(Stream file)
        {
            var strings = ReaderSkripts.BuildStringArrayByFunction(file, i =>
            {
                Match match = regex.Match(i);
                if (!match.Success)
                    throw new FileLoadException();
                return true;
            });
            return new(strings.Select(i =>
            {
                var strs = i.Split(new char[] { ' ', '\t', '\r' }).Where(i => i.Count() > 0).Select(i => i.Replace(',', '.'));
                if (strs.Count() != 2)
                    throw new FileLoadException();
                return new Point(ConvertExtension.ToDoubleRich(strs.ElementAt(0)),
                    ConvertExtension.ToDoubleRich(strs.ElementAt(1)));
            }));
        }
        public override string GetName() => Properties.Resources.PRNReaderName;
    }

    [FileReader]
    public class ReaderFileByPattern : FileReaderBase<ReaderFileByPattern>
    {
        public ReaderFileByPattern() { }
        Regex regex = new(@"^\s*-?\d+([.|,]\d*)?([e|E]-\d*)?\s*-?\d+([.|,]\d*)?([e|E]-\d*)?\s*$");
        public override PointGraph ReadFile(Stream file)
        {
            var strings = ReaderSkripts.BuildStringArrayByFunction(file, i => regex.Match(i).Success);
            return new(strings.Select(i =>
            {
                var strs = i.Split(new char[] { ' ', '\t', '\r' }).Where(i => i.Count() > 0).Select(i => i.Replace(',', '.'));
                if (strs.Count() != 2)
                    throw new FileLoadException();
                return new Point(ConvertExtension.ToDoubleRich(strs.ElementAt(0)),
                    ConvertExtension.ToDoubleRich(strs.ElementAt(1)));
            }));
        }
        public override string GetName() => Properties.Resources.ByPatternReaderName;
    }

    public class StreamData
    {
        public StreamData(Stream stream, FileMode mode)
        {
            Stream = stream;
            Mode = mode;
        }
        public Stream Stream { get; set; }
        public FileMode Mode { get; set; }
    }
    #endregion//RealizationsFileReader
}
