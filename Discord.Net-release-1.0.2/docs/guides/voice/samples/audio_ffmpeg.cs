private async Task SendAsync(IAudioClient client, string path)
{
    // Create FFmpeg using the previous example
    var ffmpeg = CreateStream(path);
    var output = ffmpeg.StandardOutput.BaseStream;
    var discord = client.CreatePCMStream(AudioApplication.Mixed);
    await output.CopyToAsync(discord);
    await discord.FlushAsync();
}
