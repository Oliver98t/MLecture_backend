import yt_dlp
import os

def download_youtube_audio(url, output_path="audio_downloads"):
    """
    Download audio from YouTube URL

    Args:
        url: YouTube video URL
        output_path: Directory to save the audio file

    Returns:
        Path to the downloaded audio file
    """
    os.makedirs(output_path, exist_ok=True)

    ydl_opts = {
        'format': 'bestaudio/best',
        'postprocessors': [{
            'key': 'FFmpegExtractAudio',
            'preferredcodec': 'mp3',
            'preferredquality': '192',
        }],
        'outtmpl': os.path.join(output_path, '%(title)s.%(ext)s'),
        'quiet': False,
        'no_warnings': False,
        'nocheckcertificate': True,  # Bypass SSL certificate verification
    }

    try:
        with yt_dlp.YoutubeDL(ydl_opts) as ydl:
            info = ydl.extract_info(url, download=True)
            filename = ydl.prepare_filename(info)
            # Replace extension with mp3
            audio_file = os.path.splitext(filename)[0] + '.mp3'
            return audio_file
    except Exception as e:
        print(f"Error downloading audio: {e}")
        return None

if __name__ == "__main__":
    youtube_url = "https://www.youtube.com/watch?v=kks2khq9XLo&list=PLDcMUvRJD9ulxT2gHbr5JgUqty5rseXfV"
    audio_file = download_youtube_audio(youtube_url)
    if audio_file:
        print(f"Audio downloaded successfully: {audio_file}")
    else:
        print("Failed to download audio")