using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Pdf;
using Windows.Data.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace source_project
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>


    public sealed partial class MainPage : Page
    {
        public ObservableCollection<FilePerformingTask> uihistory { get; set; }
        public string pathcompressionimageasset { get; set; }
        public string pathdecompressionimageasset { get; set; }
        public MainPage()
        {

            this.InitializeComponent();
            pathcompressionimageasset = "Assets/compress.png";
            pathdecompressionimageasset = "Assets/unloak.png";
            log.Visibility = Visibility.Collapsed;
            pagegrid.ColumnDefinitions.Remove(logcolumn);
            uihistory = new ObservableCollection<FilePerformingTask>();
            history.ItemsSource = uihistory;
            WriteToLog("Application is started");
        }
        public void ShowNewItemInHistory(FilePerformingTask info)
        {
            history.Items.Add(info);
        }

        public async void ShowAction()
        {
            MessageDialog dialog = new MessageDialog("Сжатие прошло успешно", "Приятная новость");
            await dialog.ShowAsync();
        }

        public void WriteToLog(string message)
        {
            log.Text += ("\n" + message);
            log.Text += ("\n" + DateTime.Now);
        }



        private async void ClickToStartCompression(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".html");
            picker.FileTypeFilter.Add(".doc");
            picker.FileTypeFilter.Add(".docx");
            picker.FileTypeFilter.Add(".pdf");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                WriteToLog(file.DisplayName + " was added");
                uihistory.Add(new FileCompressionTask(file, pathcompressionimageasset));
                uihistory[uihistory.Count - 1].RunPerforming();
                history.UpdateLayout();
                history.ItemsSource = uihistory;
            }
        }

        private async void TextBox_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    foreach (StorageFile file in items)
                    {
                        if (file.DisplayType == "Text Document")
                        {
                            WriteToLog(file.DisplayName + " was added");
                            uihistory.Add(new FileCompressionTask(file, pathcompressionimageasset));
                            uihistory[uihistory.Count - 1].RunPerforming();
                            history.UpdateLayout();
                            history.ItemsSource = uihistory;
                        } else if (file.DisplayType == "HUFF File")
                        {
                            WriteToLog(file.DisplayName + " was added");
                            uihistory.Add(new FileDecompressionTask(file, pathdecompressionimageasset));
                            uihistory[uihistory.Count - 1].RunPerforming();
                            history.UpdateLayout();
                            history.ItemsSource = uihistory;
                        } else
                        {
                            MessageDialog dialog = new MessageDialog("Данный тип файлов не поддерживается!", "Предупреждение");
                            await dialog.ShowAsync();
                        }
                    }
                }
            }
        }

        private void TextBox_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Copy...";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }

        private async void button_compress_Copy_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".huff");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                WriteToLog(file.DisplayName + " was added");
                uihistory.Add(new FileCompressionTask(file, pathcompressionimageasset));
                uihistory[uihistory.Count - 1].RunPerforming();
                history.UpdateLayout();
                history.ItemsSource = uihistory;
            }
        }



        private void button_compress_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
            var button = sender as Button;
            button.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
        }

        private void button_compress_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            var button = sender as Button;
            button.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 141, 255, 83));
        }



        private async void button_continue_Click(object sender, RoutedEventArgs e)
        {
            if (history.SelectedIndex == -1)
            {
                MessageDialog dialog = new MessageDialog("Выберите файл!", "Предупреждение");
                await dialog.ShowAsync();
            }
            else if (uihistory[history.SelectedIndex].Progress == uihistory[history.SelectedIndex].WorkInProgress)
            {
                MessageDialog dialog = new MessageDialog("Процесс уже окончен!", "Предупреждение");
                await dialog.ShowAsync();
            }
            else
            {
                if (uihistory[history.SelectedIndex].FilePerformingThread.ThreadState == System.Threading.ThreadState.Suspended)
                {
                    uihistory[history.SelectedIndex].FilePerformingThread.Resume();
                }
            }
        }

        private async void history_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            
        }


        private async void ClickToStartDecompression(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".huff");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                WriteToLog(file.DisplayName + " was added");
                uihistory.Add(new FileDecompressionTask(file, pathdecompressionimageasset));
                uihistory[uihistory.Count - 1].RunPerforming();
                history.UpdateLayout();
                history.ItemsSource = uihistory;
            }
        }

        private async void ClickToSuspendFilePerforming(object sender, RoutedEventArgs e)
        {
            if (history.SelectedIndex == -1)
            {
                MessageDialog dialog = new MessageDialog("Выберите файл!", "Предупреждение");
                await dialog.ShowAsync();
            }
            else if (uihistory[history.SelectedIndex].Progress == uihistory[history.SelectedIndex].WorkInProgress)
            {
                MessageDialog dialog = new MessageDialog("Процесс уже окончен!", "Предупреждение");
                await dialog.ShowAsync();
            }
            else
            {
                if (uihistory[history.SelectedIndex].FilePerformingThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    uihistory[history.SelectedIndex].FilePerformingThread.Suspend();
                    MessageDialog dialog = new MessageDialog("Задача приостановлена", "Оповещение");
                    await dialog.ShowAsync();
                }
            }
        }

        private void ClickCleanTaskList(object sender, RoutedEventArgs e)
        {
            uihistory.Clear();
            history.UpdateLayout();
        }

        private void ClickToShowLog(object sender, RoutedEventArgs e)
        {
            if (log.Visibility == Visibility.Visible)
            {
                log.Visibility = Visibility.Collapsed;
                LicenseGrid.SetValue(Grid.ColumnSpanProperty, 2);
                pagegrid.ColumnDefinitions.RemoveAt(2);
                LicenseGrid.UpdateLayout();
            }
            else
            {
                log.Visibility = Visibility.Visible;
                pagegrid.ColumnDefinitions.Add(new ColumnDefinition());
                LicenseGrid.SetValue(Grid.ColumnSpanProperty, 3);
                LicenseGrid.UpdateLayout();
            }
        }

        private async void ClickToShowCodes(object sender, RoutedEventArgs e)
        {
            if (history.SelectedIndex == -1)
            {
                MessageDialog dialog = new MessageDialog("Выберите файл!", "Предупреждение");
                await dialog.ShowAsync();
            }
            else if (uihistory[history.SelectedIndex].Progress < uihistory[history.SelectedIndex].WorkInProgress)
            {
                MessageDialog dialog = new MessageDialog("Процесс ещё не окончен!", "Предупреждение");
                await dialog.ShowAsync();
            }
            else
            {
                string LettersAndCodes = "codes:\n";
                for (int i = 0; i < 256; ++i)
                {
                    if (uihistory[history.SelectedIndex].codes.ContainsKey((byte)i))
                    {
                        LettersAndCodes += (char)i + "(" + i + "): " + uihistory[history.SelectedIndex].codes[(byte)i] + "\n";
                    }
                }
                WriteToLog(LettersAndCodes);
                MessageDialog dialog = new MessageDialog("Проверь log!", "Приятная новость");
                await dialog.ShowAsync();
            }
        }


        private void ClickCleanLog(object sender, RoutedEventArgs e)
        {
            log.Text = "";
        }

        private async void ClickToExportLog(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
            Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "log " + DateTime.Now;
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);

                await Windows.Storage.FileIO.WriteTextAsync(file, log.Text);

                Windows.Storage.Provider.FileUpdateStatus status =
                await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    MessageDialog dialog = new MessageDialog("Журнал " + file.DisplayName + " был сохранён", "Приятная новость");
                    await dialog.ShowAsync();
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Произошла ошибка при сохранении", "Плохая новость");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Операция отклонена", "Плохая новость");
                await dialog.ShowAsync();
            }
        }

        private async void history_DoubleTapped_1(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (history.SelectedIndex == -1)
            {
                return;
            }
            else if (uihistory[history.SelectedIndex].Progress >= uihistory[history.SelectedIndex].WorkInProgress)
            {
                var FileItem = uihistory[history.SelectedIndex];

                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add(FileItem.SuggestedFileChoose, new List<string>() { FileItem.SuggestedFileType });
                savePicker.SuggestedFileName = FileItem.Name;
                Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    byte[] BinaryFormData = FileItem.GetData();
                    Windows.Storage.CachedFileManager.DeferUpdates(file);

                    await Windows.Storage.FileIO.WriteBytesAsync(file, BinaryFormData);

                    Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        MessageDialog dialog1 = new MessageDialog("Журнал " + file.DisplayName + " был сохранён", "Приятная новость");
                        await dialog1.ShowAsync();
                    }
                    else
                    {
                        MessageDialog dialog2 = new MessageDialog("Произошла ошибка при сохранении", "Плохая новость");
                        await dialog2.ShowAsync();
                    }
                }
            }
        }
    }
}
