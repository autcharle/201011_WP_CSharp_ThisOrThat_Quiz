using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThisOrThat_Quiz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global Constants
        const int Timer_Interval = 1000;
        const int QuizTimer_Interval = 30000;
        const int colorFlashTimer_Interval = 500;
        const int delayFlashTimer_Interval = 2000;
        const int maxQuizNum = 10;
        const string N_A = "-";
        const int startQuizNum = 1;
        const int startPoints = 0;
        const int inputParams = 4;
        #endregion

        #region Global Variables
        private Timer timer;
        private Timer quizTimer;
        private Timer flashTimer;
        private Timer delayTimer;
        Color[] colors = {
            Colors.Red,
            Colors.DeepSkyBlue,
            Colors.Red,
            Colors.DeepSkyBlue
            };
        int flashTimer_Count = -1;
        int delayTimer_Flag = 0;
        int timer_Count = QuizTimer_Interval / 1000;
        int currentQuizNum = startQuizNum;
        int currentPoints = startPoints;
        List<string> quizStringLeft = new List<string>();
        List<string> quizStringRight = new List<string>();
        List<string> quizAnswer = new List<string>();
        List<string> quizQuestion = new List<string>();
        int idx = 0;
        int inGame_Flag = 0;
        List<int> quiz = new List<int>();
        #endregion

        #region Components_Loaded
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            #region Khởi tạo màn hình chờ start
            setStartDefault(); //Cài đặt giao diện
            readFile(); //Đọc dữ liệu đầu vào
            createNewQuiz(); //Tạo bộ câu hỏi
            #endregion

            #region Khởi tạo bộ đếm thời gian
            flashTimer = new Timer();
            flashTimer.Interval = colorFlashTimer_Interval;
            flashTimer.Elapsed += FlashTimer_Elapsed;
            flashTimer.Start();

            delayTimer = new Timer();
            delayTimer.Interval = delayFlashTimer_Interval;
            delayTimer.Elapsed += DelayTimer_Elapsed;
            delayTimer.Start();
            #endregion
        }
        #endregion

        #region Functions Setup
        private void Timer_Elapsed(
            object sender,
            ElapsedEventArgs e)
        {
            timer_Count--;
            Dispatcher.Invoke(() =>
                {
                    this.timerLabel.Content = $"{timer_Count}s";
                }
            );
        }

        private void QuizTimer_Elapsed(
            object sender,
            ElapsedEventArgs e)
        {
            if (currentQuizNum == maxQuizNum)
            {
                timer.Stop();
                quizTimer.Stop();
                inGame_Flag = 0;
                Dispatcher.Invoke(() =>
                {

                    setStartDefault();
                    MessageBoxResult result = MessageBox.Show(
                        $"Congrats, you finished!\nYou score is {currentPoints}/{maxQuizNum}.",
                        "Congratulations",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
                    if (result == MessageBoxResult.OK)
                    {
                        flashTimer.Start();
                        delayTimer.Start();
                    }
                    else
                    {
                        //Do nothing
                    }
                }
                );
                currentQuizNum = startQuizNum;
                currentPoints = startPoints;
            }
            else
            {
                timer_Count = QuizTimer_Interval / 1000;
                timer.Stop();
                timer.Start();
                Dispatcher.Invoke(() =>
                {
                    this.timerLabel.Content = $"{timer_Count}s";
                    this.currentQuizNumLabel.Content = updateQuizNum(ref currentQuizNum);
                    createQuiz();
                }
                );
            }
        }

        private void FlashTimer_Elapsed(
            object sender,
            ElapsedEventArgs e)
        {
            flashTimer_Count++;
            delayTimer.Stop();

            if (flashTimer_Count == colors.Length)
            {
                flashTimer_Count = -1;
                delayTimer.Start();
            }

            else
            {
                //Do nothing
            }

            Dispatcher.Invoke(() =>
            {
                this.titleLabel.Foreground = new SolidColorBrush(colors[flashTimer_Count]);
                this.startButton.Background = new SolidColorBrush(colors[flashTimer_Count]);
            }
            );
        }

        private void DelayTimer_Elapsed(
            object sender,
            ElapsedEventArgs e)
        {
            if (delayTimer_Flag == 0)
            {
                flashTimer.Stop();
                delayTimer_Flag = 1;
            }

            else
            {
                flashTimer.Start();
                delayTimer_Flag = 0;
            }
        }

        private int updateQuizNum(ref int nextQuizNum)
        {
            nextQuizNum += 1;
            return nextQuizNum;
        }

        private int updatePoints(ref int earnedPoint)
        {
            earnedPoint += 1;
            return earnedPoint;
        }

        private void setStartDefault()
        {
            this.currentQuizNumLabel.Content = N_A;
            this.currentPointsLabel.Content = N_A;
            this.quizTextLabel.Content = N_A;
            this.timerLabel.Content = $"{N_A}s";
            this.leftButton.Background = new SolidColorBrush(Colors.LightSlateGray);
            this.rightButton.Background = new SolidColorBrush(Colors.LightSlateGray);
        }

        void readFile()
        {
            const string seperator = " ";
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            var filepath = $"{folder}input.txt";

            try
            {
                var lines = File.ReadAllLines(filepath);
                foreach (var line in lines)
                {
                    var tokens = line.Split(
                        new string[] { seperator },
                        StringSplitOptions.None
                        );
                    try
                    {
                        quizStringLeft.Add(tokens[0]);
                        quizStringRight.Add(tokens[1]);
                        quizQuestion.Add(tokens[2]);
                        quizAnswer.Add(tokens[3]);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            $"Input.txt format has been changed, program can't run properly.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                            );

                        if (result == MessageBoxResult.OK)
                        {
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show(
                $"Input.txt is currently missing, program can't run properly.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
                if (result == MessageBoxResult.OK)
                {
                    this.Close();
                }
            }
        }

        void createQuiz()
        {
            string[] questions = quizStringLeft.ToArray();
            idx = quiz.First();
            quiz.RemoveAt(0);
            ImageBrush brushLeft = new ImageBrush();
            ImageBrush brushRight = new ImageBrush();
            string selectedQuiz = questions[idx];
            brushLeft.ImageSource = new BitmapImage(
                new Uri("Images/" + selectedQuiz,
                        UriKind.Relative
                        )
                );
            this.leftButton.Background = brushLeft;
            questions = quizStringRight.ToArray();
            selectedQuiz = questions[idx];
            brushRight.ImageSource = new BitmapImage(
                new Uri("Images/" + selectedQuiz,
                        UriKind.Relative
                        )
                );
            this.rightButton.Background = brushRight;
            questions = quizQuestion.ToArray();
            selectedQuiz = questions[idx];
            this.quizTextLabel.Content = selectedQuiz;
        }

        void createNewQuiz()
        {
            string[] questions = quizStringLeft.ToArray();
            int rngResult = MyRandom.Instance.Next(questions.Length);
            quiz.Clear();
            quiz.Add(rngResult);
            for (int i = 1; i < maxQuizNum; i++)
            {
                do
                {
                    rngResult = MyRandom.Instance.Next(questions.Length);
                } while (quiz.Contains(rngResult));
                quiz.Add(rngResult);
            }
        }

        void toNextQuiz()
        {
            this.currentQuizNumLabel.Content = updateQuizNum(ref currentQuizNum);
            timer.Stop();
            quizTimer.Stop();
            timer_Count = QuizTimer_Interval / 1000;
            this.timerLabel.Content = $"{timer_Count}s";
            if (currentQuizNum > maxQuizNum)
            {
                inGame_Flag = 0;
                setStartDefault();
                MessageBoxResult result = MessageBox.Show(
                    $"Congrats, you finished!\nYou score is {currentPoints}/{maxQuizNum}.",
                    "Congratulations",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
                if (result == MessageBoxResult.OK)
                {
                    flashTimer.Start();
                    delayTimer.Start();
                }
            }
            else
            {
                createQuiz();
                timer.Start();
                quizTimer.Start();
            }
        }

        #endregion

        #region Buttons_Clicked
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            //Cờ bỏ qua restart
            int restart_Flag = 1;

            #region Cài đặt mọi thứ cho một màn chơi
            createNewQuiz();

            #region Duyệt cờ để biết game đã start (Flag 1) -> Restart, hay là vẫn đang ở màn hình chờ start (Flag 0) -> Start
            if (inGame_Flag == 0) 
            {
                timer_Count = QuizTimer_Interval / 1000;
                #region Cập nhật bộ đếm
                flashTimer.Stop();
                delayTimer.Stop();
                flashTimer_Count = -1;
                delayTimer_Flag = 0;
                #endregion

                #region Khởi tạo bộ đếm thời gian theo từng giây
                timer = new Timer();
                timer.Interval = Timer_Interval;
                timer.Elapsed += Timer_Elapsed;
                #endregion

                #region Khởi tạo bộ đếm thời gian cho từng quiz
                quizTimer = new Timer();
                quizTimer.Interval = QuizTimer_Interval;
                quizTimer.Elapsed += QuizTimer_Elapsed;
                #endregion
                inGame_Flag = 1;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    "You will lose all points in this game, restart?",
                    "Restart",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Exclamation
                    );
                if (result == MessageBoxResult.OK)
                {
                    timer.Stop();
                    quizTimer.Stop();
                    timer_Count = timer_Count = QuizTimer_Interval / 1000;
                }
                else
                {
                    if (result == MessageBoxResult.Cancel)
                    {
                        restart_Flag = 0;
                        //Do nothing
                    }
                    else
                    {
                        //Do nothing
                    }
                }
            }
            #endregion

            if (restart_Flag != 0)
            {
                #region Update nội dung các component
                currentQuizNum = startQuizNum;
                currentPoints = startPoints;
                this.currentQuizNumLabel.Content = currentQuizNum;
                this.currentPointsLabel.Content = currentPoints;
                this.timerLabel.Content = $"{timer_Count}s";
                this.titleLabel.Foreground = new SolidColorBrush(Colors.White);
                this.startButton.Background = new SolidColorBrush(Colors.Gray);
                #endregion

                #region Bắt đầu bộ đếm
                timer.Start();
                quizTimer.Start();
                #endregion

                #region Hiển thị Quiz
                createQuiz();
                #endregion

            }
            #endregion
        }

        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            if (inGame_Flag == 0)
            {
                //Do nothing
            }
            else
            {
                string[] answers = quizAnswer.ToArray();

                if (answers[idx] == "left")
                {
                    this.currentPointsLabel.Content = updatePoints(ref currentPoints);
                }
                else
                {
                    //Do nothing
                }

                toNextQuiz();
            }
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            if (inGame_Flag == 0)
            {
                //Do nothing
            }
            else
            {
                string[] answers = quizAnswer.ToArray();

                if (answers[idx] == "right")
                {
                    this.currentPointsLabel.Content = updatePoints(ref currentPoints);
                }
                else
                {
                    //Do nothing
                }

                toNextQuiz();
            }
        }
        #endregion
    }

    class MyRandom
    {
        private static MyRandom _instance = null;
        private Random _rng;

        public static MyRandom Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MyRandom();
                }
                return _instance;
            }
        }

        public int Next(int ceiling)
        {
            int value = _rng.Next(ceiling);
            return value;
        }

        private MyRandom()
        {
            _rng = new Random();
        }
    }
}
