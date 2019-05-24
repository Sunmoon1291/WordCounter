using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace WordCounter
{
    class Program
    {
        static TaskRepos repos; //Репозиторий с информацией о состоянии подсчета слов в файлах      
        static object lockerPrint = new object(); //Объект блокировки вывода в консоль
        static object lockerAdd = new object(); //Объект блокировки добавления файла       
        static object lockerEdit = new object(); //Объект блокировки изменении информации о прогрессе
        static void Main(string[] args)
        {
            StreamReader objFiles;
            try
            {
                objFiles = new StreamReader("files.txt"); //Файл с перечнем обрабатываемых файлов
            }
            catch
            {
                Console.WriteLine("Файл files.txt со списком файлов не найден");
                Console.ReadKey();
                return;
            }

            string filename;
            DateTime bg = DateTime.Now;

            Console.CursorVisible = false; //Отключение отображения курсора в консоли
            repos = new TaskRepos(); //Инициализация репозитория
            repos.OnChange += PrintProgress; //Добавление обработчика события при добавлении файла или изменении информации о прогрессе

            var tl = new List<Task>(); //Список со всеми задачами. Для каждого файла своя задача

            while (!objFiles.EndOfStream)
            {
                filename = objFiles.ReadLine();
                if (filename != null && filename != "" && repos.IsNotInTasks(filename)) //для каждого не пустого имени файла, которого нет в репозитории
                {
                    tl.Add(FileProcessingAsync(filename)); //добавление файла в репозиторий
                }
            }

            objFiles.Close();

            Task.WaitAll(tl.ToArray()); //Ожидание пока не законатся все задачи по обработке файлов

            var duration = (DateTime.Now - bg).TotalMilliseconds; //Определение продолжительности выполнения
            Console.SetCursorPosition(0, repos.Tasks.AsEnumerable().Max(p => p.rownum) + 1); //Переход на новую строку
            Console.WriteLine($"Время выполнения: {duration}");
            Console.ReadKey();
        }

        public static async Task FileProcessingAsync(string filename)
        {
            lock (lockerAdd) //Если ни одна задача не добавляет файл, то добавить. Иначе ожидать пока другая задача не закончит
                repos.AddTask(filename, 0, 0, Console.CursorTop); //в последнем параметре передается номер строки в консоле, в которой будет отображаться прогресс по файлу
            await CountWords(filename); //Запуск подсчета слов
        }

        public static Task CountWords(string filename)
        {
            return Task.Run(() => //Запуск задачи по подсчету слов
            {
                StreamReader objReader = new StreamReader(filename); //Открытие файла
                int count = 0;
                Regex regexp = new Regex(@"\w+\-*\w*"); //Регулярное выражение для определения слова с учетом составных слов типа "социально-экономический"
                int rows_count = File.ReadAllLines(filename).Length; //Подсчет общего количества строк
                var i = 1;
                while (!objReader.EndOfStream)
                {
                    count += regexp.Matches(objReader.ReadLine()).Count; //добавление подсчитанных слов в строке
                    lock (lockerEdit) //если другие задачи не изменяют инф-цию о прогрессе, то изменить. Иначе ждать пока другая задача не закончит
                        repos.EditProgress(filename, i++ * 50 / rows_count, count); //прогресс определяется как отношения кол-ва обработанных строк к общему кол-ву строк
                }
            });
        }

        public static void PrintProgress(string filename)
        {
            lock (lockerPrint) //если другие потоки не выполняют вывод в консоль
            {
                var ft = repos.Tasks.Find(p => p.filename == filename); //поиск нужного файла в репозитории по имени
                var pr = ft.progress; //прогресс
                Console.SetCursorPosition(0, ft.rownum); //переход в консоли на строку соответствующую файлу
                //вывод информации о прогрессе по одному файлу, имя которого берется из параметра filename
                Console.WriteLine($"Файл: {ft.filename} [{new string('*', pr) + new string('_', 50 - pr)}] {2*pr}% Слов: {ft.words}");
            }
        }
    }
}
