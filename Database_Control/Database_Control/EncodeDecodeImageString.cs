using System.Drawing;

class ImageStringEncoderDecoder
{
    public static string encodeImageToString(string filePath)
    {
        byte[] imageArr;
        string encodedString;
        try
        {
            imageArr = File.ReadAllBytes(filePath);
            encodedString = Convert.ToBase64String(imageArr);
            return encodedString;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            return "";
        }
    }

    public static byte[] decodeBase64StringToImage( string base64String)
    {
        byte[] img = Convert.FromBase64String(base64String);
        return img;
    }

    public static Image GetImage(byte[] Data)
    {
        Image newImg;
        byte[] ImgData = (Data);
        using(MemoryStream ms = new MemoryStream(ImgData, 0, ImgData.Length))
        {
            ms.Write(ImgData, 0, ImgData.Length);
            newImg = Image.FromStream(ms, true);
        }

        return newImg;
    }

    public static byte[] ImageBytes(Image image)
    {
        MemoryStream ms = new MemoryStream();
        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
        return ms.ToArray();
    }

}