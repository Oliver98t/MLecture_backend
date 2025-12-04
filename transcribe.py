import yt_dlp
import subprocess
import os

# debug macros
DOWNLOAD = False
TRANSCRIBE = True

WHISPER_PATH = "/home/oli19/whisper.cpp/build/bin/whisper-cli"
MODEL_PATH = "/home/oli19/whisper.cpp/models/ggml-base.en.bin"

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
            'preferredcodec': 'wav',
            'preferredquality': '192',
        }],
        'postprocessor_args': [
            '-ar', '16000',  # Sample rate
            '-ac', '1',      # Mono channel
            '-sample_fmt', 's16',  # 16-bit PCM
        ],
        'outtmpl': os.path.join(output_path, '%(title)s.%(ext)s'),
        'quiet': False,
        'no_warnings': False,
        'nocheckcertificate': True,  # Bypass SSL certificate verification
    }

    try:
        with yt_dlp.YoutubeDL(ydl_opts) as ydl:
            info = ydl.extract_info(url, download=True)
            filename = ydl.prepare_filename(info)
            # Replace extension with wav
            audio_file = os.path.splitext(filename)[0] + '.wav'
            # Replace extension with wav
            audio_file = os.path.splitext(filename)[0] + '.wav'
            return audio_file
    except Exception as e:
        print(f"Error downloading audio: {e}")
        return None

def transcribe_audio(audio_file_path, whisper_path=WHISPER_PATH):
    """
    Transcribe audio file using whisper-cli

    Args:
        audio_file_path: Path to the audio file
        whisper_path: Path to whisper-cli executable

    Returns:
        Transcription text or None if failed
    """
    try:
        cmd = [whisper_path, "-f", audio_file_path, "-m", MODEL_PATH,"--output-txt", "--no-timestamps"]
        result = subprocess.run(cmd, capture_output=True, text=True)

        if result.returncode == 0:
            # Extract just the transcription text
            return result.stdout.strip()
        else:
            print(f"Transcription error: {result.stderr}")
            return None
    except Exception as e:
        print(f"Error running transcription: {e}")
        return None

if __name__ == "__main__":
    if DOWNLOAD:
        youtube_url = "https://youtu.be/WRWFpYeETHM?si=VRWbN6OwbQkafong"
        audio_file = download_youtube_audio(youtube_url)
        if audio_file:
            print(f"Audio downloaded successfully: {audio_file}")
        else:
            print("Failed to download audio")

    if TRANSCRIBE:
        text = transcribe_audio(audio_file_path="audio_downloads/test.wav")
        print(text)
