using ZXing;
using ZXing.Common;
using Microsoft.Maui.ApplicationModel; // Permissions
using Microsoft.Maui.Storage; // MediaPicker (if required in your MAUI version)
using System.IO;
using System.Threading.Tasks;


namespace GQ4
{
    public partial class MainPage : ContentPage
    {
        
        private FileResult? lastPhoto;
        string m_BarcodeScanned;
        private string  BarcodeScanned
        {
            get { return m_BarcodeScanned; }
            set {
                if (string.IsNullOrEmpty(value))
                {
                    ResultLabel.Text = "לא אותר ברקוד בצילום";
                    H0.Text = "0";
                    H1.Text = "0";
                    H2.Text = "0";
                    H3.Text = "0";

                } else
                {
                    ResultLabel.Text = value;
                    H0.Text = value.Substring(0,1);
                    H1.Text = value.Substring(1, 2);
                    H2.Text = value.Substring(3, 3);
                    H3.Text = value.Substring(4, 1);



                }

                m_BarcodeScanned = value;
            }

        }

        public MainPage()
        {
            InitializeComponent();
        }

        

        private async void OnTakePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission", "Camera permission required", "OK");
                    return;
                }

                lastPhoto = await MediaPicker.Default.CapturePhotoAsync();
                if (lastPhoto == null)
                    return;

                // Read entire photo into memory so we can both display and scan it
                using var s = await lastPhoto.OpenReadAsync();
                using var ms = new MemoryStream();
                await s.CopyToAsync(ms);
                var bytes = ms.ToArray();

                // Display image
                PhotoImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));

                // Immediately scan the captured image
                //ResultLabel.Text = "Scanning...";
                var scanText = await TryScanBytesAsync(bytes);
                if (!string.IsNullOrEmpty(scanText))
                {
                    //ResultLabel.Text = scanText;
                    BarcodeScanned = scanText;  
                }
                else
                {
                    //ResultLabel.Text = "No barcode detected.";
                    BarcodeScanned = string.Empty;
                    
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnScanImageClicked(object sender, EventArgs e)
        {
            try
            {
                if (lastPhoto == null)
                {
                    await DisplayAlert("No image", "Take a photo first", "OK");
                    return;
                }

                using var stream = await lastPhoto.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();

                ResultLabel.Text = "Scanning...";
                var scanText = await TryScanBytesAsync(bytes);
                if (!string.IsNullOrEmpty(scanText))
                    //ResultLabel.Text = scanText;
                    BarcodeScanned = scanText;
                else
                    //ResultLabel.Text = "No barcode detected.";
                    BarcodeScanned = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Scan error", ex.Message, "OK");
            }
        }

        private static async Task<string?> TryScanBytesAsync(byte[] bytes)
        {
            // Currently implemented for Android only
#if ANDROID
            try
            {
                // Decode bytes to Android bitmap
                var bmp = Android.Graphics.BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                if (bmp == null)
                    return null;

                int w = bmp.Width, h = bmp.Height;

                // Optionally downscale large images to speed up decoding
                const int maxDim = 1600;
                if (w > maxDim || h > maxDim)
                {
                    float scale = Math.Min((float)maxDim / w, (float)maxDim / h);
                    int nw = (int)(w * scale);
                    int nh = (int)(h * scale);
                    var scaled = Android.Graphics.Bitmap.CreateScaledBitmap(bmp, nw, nh, true);
                    bmp.Recycle();
                    bmp = scaled;
                    w = nw; h = nh;
                }

                var pixels = new int[w * h];
                bmp.GetPixels(pixels, 0, w, 0, 0, w, h);

                // convert int ARGB to RGB byte[] (RGB24)
                var rgb = new byte[w * h * 3];
                for (int i = 0; i < pixels.Length; i++)
                {
                    int px = pixels[i];
                    rgb[i * 3 + 0] = (byte)((px >> 16) & 0xFF); // R
                    rgb[i * 3 + 1] = (byte)((px >> 8) & 0xFF);  // G
                    rgb[i * 3 + 2] = (byte)(px & 0xFF);         // B
                }

                // Clean up bitmap
                bmp.Recycle();

                var reader = new BarcodeReaderGeneric
                {
                    AutoRotate = true,
                    Options = new DecodingOptions { TryHarder = true }
                };

                reader.Options.PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.EAN_8,
                    BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.CODE_128,  BarcodeFormat.QR_CODE, 
                    // Add more formats if needed
                };


                var zres = reader.Decode(rgb, w, h, RGBLuminanceSource.BitmapFormat.RGB24);
                if (zres != null)
                    return $"Format: {zres.BarcodeFormat}  Value: {zres.Text}";
                return null;
            }
            catch
            {
                return null;
            }
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Menu", "Cancel", null, "Settings", "Help");
            if (action == "Settings")
                await DisplayAlert("Settings", "No settings yet.", "OK");
            else if (action == "Help")
                await DisplayAlert("Help", "Use Take Photo then Scan Image.", "OK");
        }
    }
}
