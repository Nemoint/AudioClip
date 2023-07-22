// 音频文件所在的文件夹
using System.Diagnostics;
using System.Security.AccessControl;;


string? audioPath =string.Empty;
// ffmpeg 的路径
//string myFfmpegPath =" C:\\Users\\Administrator\\source\\repos\\FfmpegUsingApp\\FfmpegUsingApp\\bin\\Debug\\net6.0\\ffmpeg.exe";
string myFfmpegPath = $"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}\\ffmpeg.exe";
// 分割音频的时长（秒）
int segmentDuration = 10;
// 音频文件名
List<string> audioPathList = new List<string>();


Console.WriteLine($"请输入完整的音频文件所在的目录（可复制粘贴）,\n示例    D:\\我的音频文件夹");
//获取音频文件目录


try
{
    audioPath = Console.ReadLine();
    if (!Directory.Exists(audioPath))
    {
        while (!Directory.Exists(audioPath))
        {
            Console.WriteLine($"指定{audioPath}目录不存在，请重新输入");
            audioPath = Console.ReadLine();
        }
    }

    if (Directory.Exists(audioPath))
    {
        //如果存在此目录 要求输入切割片段时间长度
        Console.WriteLine($"请输入切割片段时间长度(单位：秒)，示例  10");
        bool segmentDurationResult = int.TryParse(Console.ReadLine(), out segmentDuration);
        if (!segmentDurationResult)
        {
            while (!segmentDurationResult)
            {
                Console.WriteLine($"输入切割片段时间长度错误，请重新输入");
                segmentDurationResult = int.TryParse(Console.ReadLine(), out segmentDuration);
            }
        }
        

        //开始遍历获取音频文件
        audioPathList = Directory.GetFiles(audioPath).ToList();

        foreach (var audioFullPath in audioPathList)
        {
            Console.WriteLine($"正在对{Path.GetFileNameWithoutExtension(audioFullPath)}进行操作.");
            //为每个音频文件创建单独的文件夹
            string folderFullPath = $"{audioPath}\\{Path.GetFileNameWithoutExtension(audioFullPath)}";
            //创建以音频文件命名的文件目录
            Directory.CreateDirectory(folderFullPath);


            // 获取音频文件的总时长（秒）
            int totalDuration = GetAudioDuration(audioFullPath, myFfmpegPath);
            // 计算分割音频的数量
            int segmentCount = (int)Math.Ceiling((double)totalDuration / segmentDuration);

            Console.WriteLine($"{folderFullPath}的音频时长为{totalDuration}，切割为{segmentCount}");

            // 循环分割音频并导出
            for (int i = 0; i < segmentCount; i++)
            {
                // 计算分割音频的起始时间（秒）
                int startTime = i * segmentDuration;
                // 计算分割音频的文件名
                string segmentName = Path.GetFileNameWithoutExtension(audioFullPath) + "_" + i.ToString("D4") + ".wav";
                // 拼接分割音频的输出路径
                // 拼接分割音频的输出路径
                string outputPath = Path.Combine(folderFullPath, segmentName);
                // 调用 ffmpeg 命令进行分割和导出
                SplitAudio(audioFullPath, outputPath, myFfmpegPath, startTime, segmentDuration);
            }
            Console.WriteLine($"操作{Path.GetFileNameWithoutExtension(audioFullPath)}完成.");

        }

        Console.WriteLine($"已完成全部音频文件操作！");
    }

}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}







// 获取音频文件的总时长（秒）
int GetAudioDuration(string audioPath, string ffmpegPath)
{
    // 创建一个 Process 对象，用于执行 ffmpeg 命令
    Process process = new Process();
    process.StartInfo.FileName = ffmpegPath;
    process.StartInfo.Arguments = $"-i \"{audioPath}\"";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.CreateNoWindow = true;
    process.Start();

    // 从标准错误流中读取 ffmpeg 的输出信息
    string output = process.StandardError.ReadToEnd();
    process.WaitForExit();

    // 从输出信息中提取音频文件的时长（格式为 HH:mm:ss.ff）
    string durationPattern = "Duration: (\\d{2}:\\d{2}:\\d{2}\\.\\d{2})";
    var match = System.Text.RegularExpressions.Regex.Match(output, durationPattern);
    if (match.Success)
    {
        // 将时长转换为秒数并返回
        TimeSpan duration = TimeSpan.Parse(match.Groups[1].Value);
        return (int)duration.TotalSeconds;
    }
    else
    {
        // 如果无法提取时长，返回 -1
        return -1;
    }
}


// 调用 ffmpeg 命令进行分割和导出音频
void SplitAudio(string inputPath, string outputPath, string ffmpegPath, int startTime, int duration)
{
    try
    {
        // 创建一个 Process 对象，用于执行 ffmpeg 命令
        Process process = new Process();
        process.StartInfo.FileName = ffmpegPath;
        process.StartInfo.Arguments = $"-ss {startTime} -t {duration} -i {inputPath} -c copy {outputPath}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        // 等待命令执行完成
        process.WaitForExit();

        process.Close();
        process.Dispose();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}