using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Microsoft.Win32;
//using System.Windows.Forms;

namespace StowTown.ViewModels
{
    public class SelectAndSaveImage
    {
        //private string selectedImagePath;
        //private string imageSource { get; set; }

        //public void SelectImage()
        //{
        //    // Create an OpenFileDialog to select an image
        //    OpenFileDialog openFileDialog = new OpenFileDialog
        //    {
        //        Title = "Select an Image",
        //        Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
        //    };

        //    // Show the dialog and check if the user selected an image
        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        // Load the selected image and display it in the SelectedImage control
        //        BitmapImage bitmap = new BitmapImage();
        //        bitmap.BeginInit();
        //        bitmap.UriSource = new Uri(openFileDialog.FileName);
        //        bitmap.EndInit();

        //        // Set the selected image as the source for the Image control
        //        //SelectedImage.Source = bitmap;
        //        imageSource = openFileDialog.FileName;
        //        selectedImagePath = Guid.NewGuid() + "_" + System.IO.Path.GetFileName(openFileDialog.FileName);

               
        //    }
        //}

        //protected void saveimage()
        //{
        //    string parentdirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        //    string saveDirectory = System.IO.Path.Combine(parentdirectory, "Images", "RadioStation");
        //    // Create the directory if it does not exist
        //    if (!Directory.Exists(saveDirectory))
        //    {
        //        Directory.CreateDirectory(saveDirectory);
        //    }

        //    // Define the complete save path
        //    string savePath = System.IO.Path.Combine(saveDirectory, selectedImagePath);

        //    try
        //    {

        //        File.Copy(imageSource, savePath, true); // Set true to overwrite if the file already exists
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error saving image: " + ex.Message);
        //    }
        //}
    }
}
