using System.Collections.Generic;
using System.Linq;

namespace WordCounter
{
    //Класс для хранения инф-ции по прогрессу для одного файла
    public class TaskProgress
    {
        public TaskProgress(string fn, int pr, int w, int rn)
        {
            filename = fn;
            progress = pr;
            words = w;
            rownum = rn;
        }
        //Имя файла
        public string filename { get; set; }
        //Прогресс число от 0 до 50
        public int progress { get; set; }
        //Количество слов в файле
        public int words { get; set; }
        //Номер строки в консоли, в которой будет отображаться прогресс по файлу
        public int rownum { get; set; }
    }

    //Класс репозиторий для хранения информации по всем файлам
    public class TaskRepos
    {
        public TaskRepos()
        {
            Tasks = new List<TaskProgress>();
        }

        //Список файлов
        public List<TaskProgress> Tasks { get; }

        //Событие изменения репозитория
        public delegate void ReposChange(string filename);
        public event ReposChange OnChange;

        //Добавление информации о файле в репозиторий
        public void AddTask(string fn, int pr, int w, int rn)
        {
            Tasks.Add(new TaskProgress(fn, pr, w, rn));
            if (OnChange != null)
                OnChange(fn);
        }

        //Проверка на то, существует ли такой файл в репозитории
        public bool IsNotInTasks(string fn)
        {
            return Tasks.AsEnumerable().Where(p => p.filename == fn).Count() == 0;
        }

        //Изменение инф-ции о прогрессе для файла
        public void EditProgress(string fn, int pr, int w)
        {
            var cur = Tasks.Find(p => p.filename == fn);
            cur.progress = pr;
            cur.words = w;
            if (OnChange != null)
                OnChange(fn);
        }
    }
}
