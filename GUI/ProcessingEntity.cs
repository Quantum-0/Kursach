using System;
using System.Collections.Generic;
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
        public string Name { get; private set;} // Filename / ArticleName
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
                    case ProcessingState.NotStarted:
                        break;
                    case ProcessingState.Processing:
                        Log($"- Статья \"{Name}\" загружена, начало обработки текста");
                        break;
                    case ProcessingState.Merging:
                        Log($"- Файл \"{Name}\" обработан, слияние с основным словарём");
                        break;
                    case ProcessingState.Finished:
                        ReadingSpeed = 0;
                        ProcessingSpeed = 0;
                        Progress = 100;
                        break;
                    case ProcessingState.Downloading:
                        Log($"- Скачивание статьи \"{Name}\"");
                        break;
                    case ProcessingState.Aborted:
                        ReadingSpeed = 0;
                        ProcessingSpeed = 0;
                        Progress = 100;
                        Log($"- Обработка статьи \"{Name}\" прервана");
                        break;
                    default:
                        break;
                }
                _State = value;
            }
        }
        public int Length { get; set; } // File/Article size
        public int ProcessedWords { private set; get; }
        public float ReadingSpeed { set; get; }
        public float ProcessingSpeed { set; get; }
        public int ProcessedChars { set; get; }
        //public int WordsFound { private set; get; }
        
        public ProcessingEntity(string Name)
        {
            this.Name = Name;
        }
        public ProcessingEntity(ArticleHeadersInfo AHI)
        {
            this.Name = AHI.Caption;
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
