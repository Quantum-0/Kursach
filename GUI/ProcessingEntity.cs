using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public enum ProcessingState
    {
        NotStarted,
        Downloading,
        Processing,
        Merging,
        Finished,
        Aborted
    }

    static class ProcessingEntitiesInfo
    {
        public static float ProcessingSpeed
        {
            get
            {
                return ProcessingEntities.Sum(p => p.ProcessingSpeed);
            }
        }
        public static float ReadingSpeed
        {
            get
            {
                return ProcessingEntities.Sum(p => p.ReadingSpeed);
            }
        }
        public static int Progress
        {
            get
            {
                var len = ProcessingEntities.Sum(p => p.Length);
                var pos = ProcessingEntities.Sum(p => p.ProcessedChars);
                return Convert.ToInt32(100 * pos / len);
            }
        }
        public static int Count
        {
            get
            {
                return ProcessingEntities.Count;
            }
        }
        /// <summary>
        /// Возвращает время обработки пачки ProcessingEntity и хранит значение ПОСЛЕ завершния обработки ДО первого считывания
        /// </summary>
        public static ulong ProcessingTime
        {
            get
            {
                if (Count == 0)
                {
                    var temp = Convert.ToUInt64(SW.ElapsedMilliseconds);
                    SW.Reset();
                    return temp;
                }
                else
                    return Convert.ToUInt64(SW.ElapsedMilliseconds);
            }
        }
        private static Stopwatch SW = new Stopwatch();
        public static List<ProcessingEntity> ProcessingEntities { get; private set; } = new List<ProcessingEntity>();
        public static void _AddEntity(ProcessingEntity PE)
        {
            if (Count == 0)
                SW.Start();
            ProcessingEntities.Add(PE);
            PE.Finished += Entity_Finished;
        }
        private static void Entity_Finished(object sender, EventArgs e)
        {
            ProcessingEntities.Remove(sender as ProcessingEntity);
            if (Count == 0)
                SW.Stop();
        }
    }

    public enum ProcessingEntityType : byte
    {
        File,
        Article
    }

    class ProcessingEntity
    {
        
        public static event EventHandler<LogEventArgs> LogEvent;
        public class LogEventArgs : EventArgs
        {
            public string Text { get; }
            public LogEventArgs(string Text)
            {
                this.Text = Text;
            }
        }
        public string Name { get; private set; } // Filename / ArticleName
        public ProcessingEntityType Type { get; }
        private byte _Progress;
        public byte Progress// 0-100%
        {
            set
            {
                if (value <= 100 && value >= 0)
                    _Progress = value;
                else
                    throw new ArgumentOutOfRangeException("Процент обработки должен быть в пределах от 0 до 100");
            }
            get
            {
                return _Progress;
            }
        }
        private ProcessingState _State;
        public ProcessingState State
        {
            get
            {
                return _State;
            }
            set
            {
                switch (value)
                {
                    case ProcessingState.Processing:
                        if (Type == ProcessingEntityType.Article)
                            Log($"- Статья \"{Name}\" загружена, начало обработки текста");
                        else
                            Log($"- Начало обработки текста из файла \"{Name}\"");
                        SW.Start();
                        break;
                    case ProcessingState.Merging:
                        if (Type == ProcessingEntityType.Article)
                            Log($"- Статья \"{Name}\" обработана, слияние с основным словарём");
                        else
                            Log($"- Файл \"{Name}\" обработан, слияние с основным словарём");
                        break;
                    case ProcessingState.Finished:
                        SW.Stop();
                        SW = null;
                        ReadingSpeed = 0;
                        ProcessingSpeed = 0;
                        Progress = 100;
                        Finished(this, EventArgs.Empty);
                        if (Type == ProcessingEntityType.Article)
                            Log($"- Обработка статьи \"{Name}\" завершена");
                        else
                            Log($"- Обработка файла \"{Name}\" завершена");
                        break;
                    case ProcessingState.Downloading:
                        if (Type == ProcessingEntityType.Article)
                            Log($"- Скачивание статьи \"{Name}\"");
                        else
                            throw new InvalidOperationException(
                                $"Невозможно установить состояние {nameof(ProcessingState.Downloading)}" +
                                $"для {nameof(ProcessingEntity)} и типом {nameof(ProcessingEntityType.File)}");
                        break;
                    case ProcessingState.Aborted:
                        SW.Stop();
                        SW = null;
                        ReadingSpeed = 0;
                        ProcessingSpeed = 0;
                        Progress = 100;
                        Finished(this, EventArgs.Empty);
                        if (Type == ProcessingEntityType.Article)
                            Log($"- Обработка статьи \"{Name}\" прервана");
                        else
                            Log($"- Обработка файла \"{Name}\" прервана");
                        break;
                    default:
                        break;
                }
                _State = value;
            }
        }
        public event EventHandler Finished;
        private Stopwatch SW;
        public long Length { get; set; } // File/Article size
        public int ProcessedWords { private set; get; }
        public float ReadingSpeed { set; get; }
        public float ProcessingSpeed { set; get; }
        public int ProcessedChars { set; get; }
        public long ProcessingTime
        {
            get
            {
                return SW.ElapsedMilliseconds;
            }
        }


        public static ProcessingEntity CreateArticle(string arcticleName)
        {
            var Entity = new ProcessingEntity(arcticleName, ProcessingEntityType.Article);
            ProcessingEntitiesInfo._AddEntity(Entity);
            return Entity;
        }
        public static ProcessingEntity CreateArticle(ArticleHeadersInfo arcticleInfo)
        {
            var Entity = new ProcessingEntity(arcticleInfo.Caption, ProcessingEntityType.Article);
            ProcessingEntitiesInfo._AddEntity(Entity);
            return Entity;
        }
        public static IEnumerable<ProcessingEntity> CreateArticles(IEnumerable<ArticleHeadersInfo> articleInfos)
        {
            foreach (var a in articleInfos)
            {
                yield return CreateArticle(a);
            }
            yield break;
        }
        public static ProcessingEntity CreateFile(string fileName)
        {
            var Entity = new ProcessingEntity(fileName, ProcessingEntityType.File);
            ProcessingEntitiesInfo._AddEntity(Entity);
            return Entity;
        }
        public static ProcessingEntity CreateFile(FileInfo fileInfo)
        {
            var Entity = new ProcessingEntity(fileInfo.Name, ProcessingEntityType.File);
            Entity.Length = fileInfo.Length;
            ProcessingEntitiesInfo._AddEntity(Entity);
            return Entity;
        }
        public static IEnumerable<ProcessingEntity> CreateFiles(IEnumerable<FileInfo> fileInfos)
        {
            foreach (var f in fileInfos)
            {
                yield return CreateFile(f);
            }
            yield break;
        }
        private ProcessingEntity(string Name, ProcessingEntityType Type)
        {
            this.Name = Name;
            this.Type = Type;
            SW = new Stopwatch();
        }
        
        public void Log(string Text)
        {
            LogEvent?.Invoke(this, new LogEventArgs(Text));
        }
        public void AddWord()
        {
            ProcessedWords++;
        }
    }

    class ArticleHeadersInfo
    {
        public string Caption { get; private set; }
        public string Id { get; private set; }

        public ArticleHeadersInfo(string Id, string Caption)
        {
            this.Id = Id;
            this.Caption = Caption;
        }
        public static ArticleHeadersInfo[] CreateArray(string[] Ids, string[] Captions)
        {
            return Ids.Zip(Captions, (id, capt) => new ArticleHeadersInfo(id, capt)).ToArray();
        }
    }
}
