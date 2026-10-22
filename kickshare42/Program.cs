using System;
using System.Windows.Forms;

namespace _42v2
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                CurrentUser.Reset();

                using (LoginForm login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                    {
                        // Если на форме входа нажали крестик — закрываем программу
                        break;
                    }
                }

                Form1 mainForm = new Form1();
                Application.Run(mainForm);

                // Если вышли НЕ через кнопку "Выйти", а закрыли окно на крестик — завершаем цикл
                if (!mainForm.IsLoggedOut)
                {
                    break;
                }
            }
        }
    }
}
