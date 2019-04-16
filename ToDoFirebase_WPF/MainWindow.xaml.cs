using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ToDoFirebase_WPF
{

    public partial class MainWindow : Window
    {


        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "yourauthsecret",
            BasePath = "https://yourprojectname.firebaseio.com/"
        };
        IFirebaseClient client;



        public MainWindow()
        {
            InitializeComponent();
            
            ListenToStream();
        }


        private static Dictionary<string, string> keyHolder = new Dictionary<string, string>();
        private async void ListenToStream()
        {

            client = new FirebaseClient(config);

            EventStreamResponse response = await client.OnAsync("FireSharp/Value/", (sender, args, context) => {
                string dataFromFB = args.Data;
                string paths = args.Path;
                string key = RemoveNameSubstring(paths);
                string uniqueKey = key.Split('/').Last();
                if (keyHolder.ContainsKey(uniqueKey))
                {
                    keyHolder[uniqueKey] = dataFromFB;
                    AddToListView(dataFromFB);
                }
                else
                {
                    keyHolder.Add(uniqueKey, dataFromFB);
                    AddToListView(dataFromFB);
                }

            });

            //response.Dispose(); 
        }

        private void AddToListView(string data)
        {
            Dispatcher.Invoke(() =>
            {
                if (send.Content.Equals("Update"))
                {
                    listView.Items.Insert(listView.SelectedIndex, data);
                    listView.Items.RemoveAt(listView.SelectedIndex);
                    send.Content = "Send";
                }
                else
                {
                    listView.Items.Add(data);
                }

            });

        }


        private async void SendDataToFirebase(string text)
        {
            client = new FirebaseClient(config);


            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("TaskValue", text);

            var response = await client.PushAsync("FireSharp/Value/", values);
            textBox.Text = "";
        }
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> val = new Dictionary<string, string>();
            string selectedIndex;
            string pathToUpdate;


            if (send.Content.Equals("Update"))
            {
                selectedIndex = keyHolder.Keys.ElementAt(listView.SelectedIndex);

                pathToUpdate = selectedIndex;
                if (textBox.Text == "")
                {
                    MessageBox.Show("Empty value!", "Oops!");

                }
                else
                {
                    val["TaskValue"] = textBox.Text;
                    UpdateList(pathToUpdate, val);
                }

            }
            else
            {
                if (textBox.Text == "")
                {
                    MessageBox.Show("Empty value!", "Oops!");
                }
                else
                {
                    SendDataToFirebase(textBox.Text);
                }
            }

        }
        private async void DeleteDataFromFirebase(string val)
        {
            client = new FirebaseClient(config);
            FirebaseResponse delete = await client.DeleteAsync("FireSharp/Value/" + val);
            var status = delete.StatusCode;

            if (status.ToString() == "OK")   // if returned OK status, value is removed from ListView
            {
                listView.Items.Remove(listView.SelectedItem);
                keyHolder.Remove(val);
            }

        }

        private async void UpdateList(string key, Dictionary<string, string> updatedValue)
        {
            FirebaseResponse response;


            response = await client.UpdateAsync("FireSharp/Value/" + key, updatedValue);

            if (listView.SelectedIndex == 0 || listView.SelectedIndex == keyHolder.Count - 1)
            {
                keyHolder[keyHolder.Keys.ElementAt(listView.SelectedIndex)] = textBox.Text;
                listView.Items.Insert(listView.SelectedIndex, textBox.Text);
                listView.Items.RemoveAt(listView.SelectedIndex);
                send.Content = "Send";


            }

            textBox.Text = "";

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

            if (listView.SelectedItem == null)
            {
                MessageBox.Show("Please select an Item to Remove", "Sorry");
            }

            else
            {
                int currentIndex = listView.SelectedIndex;

                string currentKey = keyHolder.Keys.ElementAt(currentIndex);
                if (listView.SelectedItem.ToString() == keyHolder.Values.ElementAt(currentIndex))
                {
                    DeleteDataFromFirebase(currentKey);

                }
            }
        }
        public string RemoveNameSubstring(string name)
        {
            int index = name.IndexOf("/TaskValue");
            string uniqueKey = (index < 0) ? name : name.Remove(index, "/TaskValue".Length);
            return uniqueKey;


        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

            if (listView.SelectedItem == null)
            {
                MessageBox.Show("Please select an Item to Update", "Sorry");
            }
            else
            {
                textBox.Text = listView.SelectedItem.ToString();
                send.Content = "Update";
            }
        }



    }
}
