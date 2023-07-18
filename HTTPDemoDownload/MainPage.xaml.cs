using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HTTPDemoDownload
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    StorageFile downloadedFile;
    public MainPage()
    {
      this.InitializeComponent();
    }

    private async void btnDownload_Click(object sender, RoutedEventArgs e)
    {
      string link = txtLink.Text.Trim();

      if (!string.IsNullOrEmpty(txtLink.Text))
      {
        try
        {
          string fileName = GetFileNameFromUrl(link);
          string fileExtension = GetFileExtensionFromUrl(link);

          if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileExtension))
          {
            StorageFolder folder = await PickFolder();

            if (folder != null)
            {
              DisableDownloadButton();

              Uri address = new Uri(txtLink.Text, UriKind.Absolute);
              HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
              WebResponse response = await request.GetResponseAsync();
              Stream stream = response.GetResponseStream();

              //Uri address = new Uri(txtLink.Text, UriKind.Absolute);
              //HttpClient client = new HttpClient();
              //HttpResponseMessage response = await client.GetAsync(address);
              //Stream stream = await response.Content.ReadAsStreamAsync();

              StorageFile file = await SaveStreamToFile(stream, folder, fileName, fileExtension);
              downloadedFile = file;

              EnableDownloadButton();

              if (downloadedFile != null)
                await Windows.System.Launcher.LaunchFileAsync(downloadedFile);
            }
          }
          else
          {
            MessageDialog dialog = new MessageDialog("Could not determine the name and the extension of the file");
            await dialog.ShowAsync();
          }
        }
        catch (Exception ex)
        {
          MessageDialog dialog = new MessageDialog("An error occurred while downloading the file " + ex.Message);
          await dialog.ShowAsync();
          ResetDownload();
        }
      }
    }
    private async void btnOpenFile_Click(object sender, RoutedEventArgs e)
    {
      if (downloadedFile != null)
      {
        var messageDialog = new MessageDialog("Do you want to open the downloaded file?", "Confirmation");
        messageDialog.Commands.Add(new UICommand("Yes", async (command) =>
        {
          await Windows.System.Launcher.LaunchFileAsync(downloadedFile);
        }));
        messageDialog.Commands.Add(new UICommand("No"));

        await messageDialog.ShowAsync();
      }
    }
    private async Task<Stream> DownloadFile(string addressText)
    {
      Uri address = new Uri(addressText, UriKind.Absolute);
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
      WebResponse response = await request.GetResponseAsync();
      Stream stream = response.GetResponseStream();

      //Uri address = new Uri(addressText, UriKind.Absolute);
      //HttpClient client = new HttpClient();
      //HttpResponseMessage response = await client.GetAsync(address);
      //Stream stream = await response.Content.ReadAsStreamAsync();
      return stream;
    }
    private async Task<StorageFolder> PickFolder()
    {
      FolderPicker folderPicker = new FolderPicker();
      folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
      folderPicker.ViewMode = PickerViewMode.Thumbnail;
      folderPicker.FileTypeFilter.Add("*");

      return await folderPicker.PickSingleFolderAsync();
    }
    private async Task<StorageFile> SaveStreamToFile(Stream stream, StorageFolder folder, string fileName, string fileExtension)
    {
      StorageFile file = await folder.CreateFileAsync(fileName + fileExtension, CreationCollisionOption.GenerateUniqueName);
      byte[] buffer = ReadStream(stream);
      await FileIO.WriteBytesAsync(file, buffer);
      return file;
    }
    private byte[] ReadStream(Stream stream)
    {
      byte[] buffer = new byte[16 * 1024];
      using (MemoryStream ms = new MemoryStream())
      {
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
          ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
      }

    }
    private string GetFileNameFromUrl(string url)
    {
      try
      {
        return Path.GetFileNameWithoutExtension(url);
      }
      catch
      {
        return null;
      }
    }
    private string GetFileExtensionFromUrl(string url)
    {
      try
      {
        return Path.GetExtension(url);
      }
      catch
      {
        return null;
      }
    }
    private void ResetDownload()
    {
      btnDownload.IsEnabled = true;
      ProgressBar.Visibility = Visibility.Collapsed;
      ProgressBar.IsIndeterminate = false;
      txtDownloadCompleted.Visibility = Visibility.Collapsed;
      downloadedFile = null;
    }
    private void DisableDownloadButton()
    {
      btnDownload.IsEnabled = false;
      ProgressBar.IsIndeterminate = true;
      ProgressBar.Visibility = Visibility.Visible;
    }
    private void EnableDownloadButton()
    {
      btnDownload.IsEnabled = true;
      ProgressBar.Visibility = Visibility.Collapsed;
      ProgressBar.IsIndeterminate = false;
      txtDownloadCompleted.Visibility = Visibility.Visible;
      //btnOpenFile.Visibility = Visibility.Visible;
    }
    private async void ShowErrorMessage(string message)
    {
      MessageDialog dialog = new MessageDialog(message);
      await dialog.ShowAsync();
    }
  }
}
