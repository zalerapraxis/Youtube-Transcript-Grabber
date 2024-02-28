using HtmlAgilityPack;
using System.Runtime.InteropServices;
using System.Text;
using YoutubeTranscriptApi;

public class YouTubeTranscriptFetcher
{

    static void Main()
    {
        Console.WriteLine("Paste in the Youtube URL here: ");
        var input = Console.ReadLine();

        if (!input.Any())
        {
            Console.WriteLine("I didn't get a URL. Try again.");
            Main();
            return;
        }

        // Fetch transcript
        string transcript = GetTranscriptAsync(input);

        // Check if we error'd out - re-run if we did
        if (transcript == null)
        {
            Main();
            return;
        }

        // Print transcript
        Console.WriteLine("The transcript has been copied to your clipboard.");
        CopyTextToClipboard(transcript);
    }

    static string GetTranscriptAsync(string videoUrl)
    {
        try
        {
            var api = new YouTubeTranscriptApi();

            // Get video title
            var videoTitle = GetYouTubeVideoTitle(videoUrl);

            // Extract video ID from URL
            string videoId = ParseVideoId(videoUrl);
            if (videoId == null)
            {
                Console.WriteLine("It doesn't look like we could find a video from that URL. Try again.");
                return null;
            }

            // Grab subtitles from video
            var response = api.GetTranscript(videoId);

            // combine transcript snippets into one string
            var transcript = new StringBuilder();
            foreach (var snippet in response)
            {
                transcript.Append(snippet.Text + " ");
            }

            // add ML prompt to end of transcript
            transcript.AppendLine(Environment.NewLine);
            transcript.AppendLine("---");
            transcript.AppendLine(Environment.NewLine);
            transcript.AppendLine($"This is the transcript of a YouTube video titled '{videoTitle}'. Tell me what it's about in a super simple and concise way while using the same language.");

            return transcript.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while retrieving the video transcript: {ex.Message}");
            return null;
        }
    }

    static string ParseVideoId(string videoUrl)
    {
        // Extract video ID from the URL
        Uri uri = new Uri(videoUrl);
        string videoId = uri.Query.TrimStart('?').Split('&')
                           .Select(x => x.Split('='))
                           .FirstOrDefault(x => x[0] == "v")?[1];
        return videoId;
    }

    static string GetYouTubeVideoTitle(string url)
    {
        try
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            // XPath to select the title element of the YouTube page
            string xpath = "//meta[@name='title']";
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode(xpath);

            if (titleNode != null)
            {
                // Extracting the title attribute
                string title = titleNode.Attributes["content"].Value;
                return title;
            }
            else
            {
                return "Title not found";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while parsing the video title: " + ex.Message);
            return null;
        }
    }

    static void CopyTextToClipboard(string text)
    {
        try
        {
            // Open and clear the clipboard
            if (!OpenClipboard(IntPtr.Zero))
            {
                throw new Exception("Failed to open clipboard.");
            }

            EmptyClipboard();

            // Allocate global memory
            IntPtr hGlobal = Marshal.StringToHGlobalUni(text);

            // Set the data to clipboard
            if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
            {
                throw new Exception("Failed to set clipboard data.");
            }

            // Close the clipboard
            CloseClipboard();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying text to clipboard: {ex.Message}");
        }
    }

    // Constants required for clipboard operations
    const uint CF_UNICODETEXT = 13;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EmptyClipboard();

    [DllImport("kernel32.dll")]
    static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll")]
    static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    static extern IntPtr GlobalFree(IntPtr hMem);
}
