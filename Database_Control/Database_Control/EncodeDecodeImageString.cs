using System.Drawing;

class ImageStrinEncoderDecoder
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
            // modify this to see if you can integrate into UI
            Console.WriteLine("Error: " + ex.Message);
            return null;
        }
    }

    public static byte[] decodeBase64StringToImage( string base64String)
    {
        byte[] img = Convert.FromBase64String(base64String);
        return img;
    }

}