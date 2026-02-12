using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues.Models;
using Azure.Data.Tables;
using System.Text.Json;
using Common;

namespace Company.Function;

class TranscriptAPI
{
    public static async Task<string> TranscribeCall(string youtubeUrl)
    {
        string? TranscriptApiKey = Environment.GetEnvironmentVariable("TranscriptApiKey");
        string requestUrl = $"https://transcriptapi.com/api/v2/youtube/transcript?video_url={youtubeUrl}&format=text&include_timestamp=false";
        HttpClient client = new HttpClient();
        string content;
        if(GlobalConsts.TranscriptAPIEnable == "true")
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TranscriptApiKey}");
            var response = await client.GetAsync(requestUrl);
            content = await response.Content.ReadAsStringAsync();
        }
        else
        {
            content = @"{""video_id"":""p30tWWEElxU"",""language"":""en"",""transcript"":""so hopefully you've watched my video on one-dimensional Suva using Newton's equations of motion now let's move on to two dimensions that is projectile motion okay so let's say that Bob wants to have a go at some tombstoning as they call it so running off the edge of a cliff and he wants to get as far as you can into the water so here's Bob right here he's about to run off there I was going to take this trajectory into the water now I can tell you Bob it's pretty fast so he's running with a speed of 8 meters per second I want to know how far he actually gets I'm gonna know his distance there I'm going to call that D now the first thing we need to do is separate Bob's motion into vertical and horizontal components whenever something's thrown up into the air at whatever angle we know that whatever's going on vertically with its speed and acceleration is going to be completely independent of whatever is going on horizontally let's say accidentally he trips and as opposed to purposefully tripping he trips here and then he just whoop falls right down a cliff so he lands right here that's actually going to take him the same amount of time therefore showing that it doesn't matter how fast he's going he's always going to take the same amount of time to get down to the water okay so let's write down what we know is going on suvat wise vertically this is how you lay out your question so you're right down your Suva as per usual I can tell you that this cliff is 50 meters high very very high he's a bit of a daredevil is Bob so therefore we can write down his displacement vertically is 50 meters do we know anything else about Bob's movement vertically well we should do because like we saw on the last video whenever something just is going horizontally or just starts dropping we know that its initial speed initial velocity rather zero meters per second this is the bit where people get confused they get confused between initial velocity vertically and the speed at which something is traveling horizontally when it starts falling whenever something is going horizontally off an edge you can always say that its initial vertical velocity is going to be zero we don't know V so we're going to forget about that acceleration well he's falling under gravity so we know a is going to be 9.8 meters per second squared we're trying to find out time so we need to find out at what time is here so we're going to use the equation s equals UT plus half a T squared now just as an aside this equation you're only going to get given this equation if u is zero if you're trying to find out T if you wasn't zero then you're going to end up with T squared times something plus T times something and then you're going to have to use the quadratic formula to actually figure it out but you don't have to do that at GCC or a level physics so you're going to get this you're going to have to use this to find out t only if u is zero so we know U is zero so we can get rid of U T there and we need to rearrange it to find T so let's do this one step at a time to s equals a T squared so I can say two s divided by a is T squared finally that means square root and the whole lot gets us T is the square root of 2 s divided by a pop that into my calculator so I'm going to have the square root of two times 50 divided by 9.8 that gives me a time of 3.1 9 seconds great so now that I know how long it takes Bob to actually get down to the water I can actually use this time this is usually the way it goes it's the time that links the vertical and horizontal components together because it cause distance speed and acceleration can be different horizontal and vertical but time is always going to be the same between the two so I'm going to carry this over to my horizontal calculations now now of course we know that if something is traveling horizontally it's not going to keep on going forever because of course you have air resistance but we forget about hair resistance because we just pretend that isn't there no air resistance at all no drag forces now if there's no air resistance then that means that there's no forces acting horizontally on Bob at all the only force acting on him his is weight pulling him downwards therefore we can say the speed is constant if speed is constant then that means acceleration is going to be zero so actually that means that there's no need for suvat so that's a good thing about projectile motion at GCSE and a-level you never have to use suvat in both vertical and horizontal components you only have to do it vertically so if you know speed is constant then we can just say speed equals distance over time that means therefore the distance is speed times time and of course when we talk about speed we're talking about is horizontal speed that horizontal speed it started off at you meters per second that's always going to be the same so we're going to say eight times three point one nine that gives us a distance of 25 point six meters therefore we can say that D is 25 point six meters that's how we do project our motion we use C that vertically to find out the time usually this is the way it goes and then we use that time to put that into our speed equals distance over time equation then find out a distance alternatively you could get given the distance and the speed here and to figure out the time and then pop that back in to you'll see that equation so just be aware it's the time it's the only thing that's going to be constant between horizontal and vertical components of the motion what if you were asked what is Bob's final speed final velocity as he enters the water now when we're talking about his final velocity we're not talking about arteries hit the water but what speed he actually hits the water with so of course we're talking about the speed before he slowed down by the water itself now here's Bob as he's about to go into the water now we know horizontally that his speed is 8 meters per second if I wanted to find out his vertical velocity as he enters the water that's obviously going to be our V now we could use V equals U plus 18 but generally I would suggest that because we've actually calculated what T is then I wouldn't use that let's use the information that we've actually been given in the question so we can avoid as many possible problems so we're going to use V squared equals u squared plus 2 a s because we know that U is 0 so that is P R so V is going to be the square root of 2 a s that's going to be the square root of 2 times 9.8 times 50 so that gives us a final downwards velocity of 30 1.3 m/s so I can put that on there now instead of video can say that's 30 1.3 m/s we know whose final velocity is obviously going to be in this direction how do we find that out well as per usual like you've seen before what we do is make ourselves a triangle if you really want to so you just pop this around here and we have ourselves a triangle all we have to do then is use Pythagoras let's call this speed I don't know 18 that we've already used V a is just going to be the square root of a squared plus 31.3 squared that gives us a final speed of 30 2.3 m/s what about if you wanted to find out what angle he hits the water at well technically the angle to the horizontal is going to be that there let's call that theta for now and of course that's going to be the same there so we have our triangle and obviously we have our adjacent and we have our opposite so therefore theta is going to be equals to the inverse tan of our opposite / our adjacent that's going to give us a fairly big angle of seventy five point seven degrees this is just for a level now what if we had something actually being fired up at an angle let's say we've got a cannon here and it's going to be firing a shell upwards at an angle of 30 degrees let's say it fires it at a speed of 80 meters per second of course as usual we have to split this into horizontal and vertical components in order for us to do anything useful with it obviously we're not going to have components that are bigger than 80 meters per second they're always going to be smaller so we're going to have to times by COS of 30 or sine of 30 but which way round is it well of course like from our rule in our previous video if you turn through the angle then we use cause if we're turning away from our angle we're turning away from our sin turning away from our sine so this horizontal velocity as it leaves the cannon is going to be 80 because 30 and then the vertical one is going to be 80 sine 30 so we can say horizontal is that you vertical is that now of course if there's no air resistance this is going to stay constant and we don't have to use see that's horizontally at all so therefore that's going to stay the same but then vertically obviously we've got to do something with that this speed it turns out is 40 meters per second now of course if we're saying that upwards is positive downwards is negative when it comes back down the other side so obviously it's taking this trajectory here by the time it reaches the other side we know that its final vertical velocity is going to be minus 40 meters per second just like we saw with me throwing the ball up in the air in the previous video you have your you you have your V you know what a is therefore you can find out what time and the displacement are going to be obviously the displacement vertically is going to be zero so that we don't really use that at all what we usually do is find out the time it takes for it to go up and come back down and then we use that as per usual to find out how far it's gone of course a in this case it's going to be minus 9.8 meters per second squared because it's accelerating downwards even though it's going upwards to begin with so that's the difficult bit as soon as you resolve your resultant speed velocity into horizontal and vertical components you treat it just the same as the previous question that we saw with Bob jumping off the cliff so there you go I hope you found this video helpful if you did then please make sure you leave a like and make sure you leave a comment as well down below suggesting what I can cover next and I'll see you next time""}";
        }

        var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        string? transcript = root.GetProperty("transcript").GetString();
        return $"{transcript}\n";
    }
}

public class Transcribe
{
    private readonly ILogger<Transcribe> _logger;
    private readonly TableServiceClient _tableServiceClient;

    public Transcribe(ILogger<Transcribe> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    public class TranscribeResponse
    {
        [QueueOutput(QueueConsts.transcriptionResults, Connection = "AzureWebJobsStorage")]
        public TranscribeTriggerQueueData? ResultsMessage { get; set; }
    }

    private async Task AddTranscriptToDB(string user, string transcript, string jobId)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableConsts.transcriptionTable);
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity("User", Guid.NewGuid().ToString())
        {
            { "User", user },
            { "Transcript", transcript },
            { "JobId", jobId },
            { "Timestamp", DateTime.UtcNow }
        };
        await tableClient.AddEntityAsync(entity);
    }

    [Function("TranscribeTrigger")]
    public async Task<TranscribeResponse> TranscribeTrigger(
        [QueueTrigger(QueueConsts.transcriptionJob, Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        StartCreateNotesQueueData? inputQueueData = JsonSerializer.Deserialize<StartCreateNotesQueueData>(message.MessageText);
        TranscribeTriggerQueueData? outputQueueData;
        if( inputQueueData != null )
        {
            string transcript = await TranscriptAPI.TranscribeCall(inputQueueData.Url);
            outputQueueData = new(inputQueueData.User, transcript, inputQueueData.JobId);
            _logger.LogInformation(transcript);
            await AddTranscriptToDB(inputQueueData.User, transcript, inputQueueData.JobId);
        }
        else
        {
            outputQueueData = null;
        }

        return new TranscribeResponse
        {
            ResultsMessage = outputQueueData,
        };
    }
}