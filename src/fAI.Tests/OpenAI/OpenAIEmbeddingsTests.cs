using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class OpenAIEmbeddingsTests
    {
        [Fact()]
        public void Embeddings_Create()
        {
            var input = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            var client = new OpenAI();
            var r = client.Embeddings.Create(input);
            Assert.Equal("list", r.Object);
            Assert.Single(r.Data);
            Assert.Equal("embedding", r.Data[0].Object);
            Assert.Equal(0, r.Data[0].Index);
            Assert.Equal(r.Data[0].EmbeddingMaxValue, r.Data[0].Embedding.Count);
            Assert.Equal(37, r.Usage.PromptTokens);
            Assert.Equal(37, r.Usage.TotalTokens);
        }

        [Fact()]
        public void Embeddings_CreateBatch()
        {
            var songLyrics = Revolver.Values.ToList().Skip(1).Take(12).ToList();
            var client = new OpenAI();
            var r = client.Embeddings.CreateBatch(songLyrics);
            Assert.Equal(12, r.Count);
            foreach(var item in r)
            {
                Assert.Equal("list", item.Object);
                Assert.Single(item.Data);
                Assert.Equal("embedding", item.Data[0].Object);
                Assert.Equal(0, item.Data[0].Index);
                Assert.Equal(item.Data[0].EmbeddingMaxValue, item.Data[0].Embedding.Count);

                foreach (var f in item.Data[0].Embedding)
                    Assert.InRange(f, -1.0f, 1.0f); // Check that the embedding values are in the expected range

                Assert.True(item.Usage.PromptTokens > 100);
                Assert.True(item.Usage.TotalTokens> 100);
            }
        }

        Dictionary<string, string> Revolver = new Dictionary<string, string>()
        {
            ["Revolver - Beatles"] = @"
The Beatles' album ""Revolver"" is often hailed as one of the greatest and most influential albums in the history of rock music. Released on 5th August 1966, it marked a significant period of creative evolution for the band, showcasing their willingness to experiment with new sounds and recording techniques.

Key Features and Innovations:

- Musical Diversity: ""Revolver"" features an eclectic mix of styles, ranging from pop and rock to psychedelic, classical, and Indian music. This diversity reflects the Beatles' growing interests in different musical genres and their desire to break away from traditional pop formulas.

- Studio Experimentation: The album is notable for its innovative studio techniques, including the use of backward tape loops, varispeed recording, and new effects like automatic double tracking (ADT). These techniques contributed to the album's distinctive soundscapes and demonstrated the studio's potential as an instrument.

- Lyrics and Themes: The lyrics on ""Revolver"" delve into more complex and introspective themes compared to the band's earlier work. Songs explore topics such as love, death, loneliness, and social commentary, showcasing the Beatles' maturing songwriting.

- Iconic Tracks: ""Revolver"" features some of the Beatles' most beloved songs, including ""Eleanor Rigby,"" known for its string octet and narrative storytelling; ""Tomorrow Never Knows,"" a groundbreaking track that incorporated tape loops and Indian-inspired drone; and ""Yellow Submarine,"" a whimsical and colorful song that became the title for their 1968 animated film.

- Cover Art: The album's cover, designed by Klaus Voormann, a friend of the band from their days in Hamburg, is a collage of drawn images and photographs of the Beatles. It won a Grammy Award for Best Album Cover, Graphic Arts and is considered iconic in the realm of album artwork.

""Revolver"" represents a pivotal moment in The Beatles' career and the 1960s musical landscape, setting new standards for what could be achieved in popular music. It's widely regarded as a masterpiece and continues to influence artists across various genres.
",

            ["Taxman"] = @"
""Taxman""
1,2,3,4
Hrmm!
1,2...
1,2,3,4.

Let me tell you how it will be
There's one for you, nineteen for me
Cos I'm the taxman, yeah, I'm the taxman.

Should five per cent appear too small
Be thankful I don't take it all
Cos I'm the taxman, yeah I'm the taxman.

If you drive a car, I'll tax the street
If you try to sit, I'll tax your seat
If you get too cold I'll tax the heat
If you take a walk, I'll tax your feet.

Taxman!
Cos I'm the taxman, yeah I'm the taxman.

Don't ask me what I want it for (Aahh Mr. Wilson)
If you don't want to pay some more (Aahh Mr. Heath)
Cos I'm the taxman, yeah, I'm the taxman.

Now my advice for those who die
Declare the pennies on your eyes
Cos I'm the taxman, yeah, I'm the taxman.

And you're working for no one but me
Taxman!
",

            ["Eleanor Rigby"] = @"
Ah, look at all the lonely people
Ah, look at all the lonely people

Eleanor Rigby picks up the rice in the church where a wedding has been
Lives in a dream
Waits at the window, wearing the face that she keeps in a jar by the door
Who is it for?

All the lonely people
Where do they all come from?
All the lonely people
Where do they all belong?

Father McKenzie writing the words of a sermon that no one will hear
No one comes near
Look at him working, darning his socks in the night when there's nobody there
What does he care?

All the lonely people
Where do they all come from?
All the lonely people
Where do they all belong?

Ah, look at all the lonely people
Ah, look at all the lonely people

Eleanor Rigby died in the church and was buried along with her name
Nobody came
Father McKenzie wiping the dirt from his hands as he walks from the grave
No one was saved

All the lonely people
(Ah, look at all the lonely people)
Where do they all come from?
All the lonely people
(Ah, look at all the lonely people)
Where do they all belong?",

            ["I'm Only Sleeping"] = @"
When I wake up early in the morning
Lift my head, I'm still yawning
When I'm in the middle of a dream
Stay in bed, float up stream (Float up stream)

Please, don't wake me, no, don't shake me
Leave me where I am, I'm only sleeping

Everybody seems to think I'm lazy
I don't mind, I think they're crazy
Running everywhere at such a speed
Till they find there's no need (There's no need)

Please, don't spoil my day, I'm miles away
And after all I'm only sleeping

Keeping an eye on the world going by my window
Taking my time

Lying there and staring at the ceiling
Waiting for a sleepy feeling...

Please, don't spoil my day, I'm miles away
And after all I'm only sleeping

Ooh yeah

Keeping an eye on the world going by my window
Taking my time

When I wake up early in the morning
Lift my head, I'm still yawning
When I'm in the middle of a dream
Stay in bed, float up stream (Float up stream)

Please, don't wake me, no, don't shake me
Leave me where I am, I'm only sleeping",

            ["Love You To"] = @"
Each day just goes so fast
I turn around, it's past
You don't get time to hang a sign on me

Love me while you can
Before I'm a dead old man

A lifetime is so short
A new one can't be bought
But what you've got means such a lot to me

Make love all day long
Make love singing songs

Make love all day long
Make love singing songs

There's people standing round
Who'll screw you in the ground
They'll fill you in with all their sins, you'll see

I'll make love to you
If you want me to
",


            ["Here, There And Everywhere"] = @"
To lead a better life I need my love to be here...

Here, making each day of the year
Changing my life with the wave of her hand
Nobody can deny that there's something there

There, running my hands through her hair
Both of us thinking how good it can be
Someone is speaking but she doesn't know he's there

I want her everywhere and if she's beside me
I know I need never care
But to love her is to need her everywhere
Knowing that love is to share

Each one believing that love never dies
Watching her eyes and hoping I'm always there

I want her everywhere and if she's beside me
I know I need never care
But to love her is to need her everywhere
Knowing that love is to share

Each one believing that love never dies
Watching her eyes and hoping I'm always there

I will be there and everywhere
Here, there and everywhere
",
            ["Yellow Submarine"] = @"
In the town where I was born
Lived a man who sailed to sea
And he told us of his life
In the land of submarines

So we sailed up to the sun
'Til we found the sea of green
And we lived beneath the waves
In our yellow submarine

We all live in a yellow submarine
Yellow submarine, yellow submarine
We all live in a yellow submarine
Yellow submarine, yellow submarine

And our friends are all aboard
Many more of them live next door
And the band begins to play

We all live in a yellow submarine
Yellow submarine, yellow submarine
We all live in a yellow submarine
Yellow submarine, yellow submarine

Full speed ahead, Mr. Boatswain, full speed ahead!
Full speed it is, Sergeant!
Cut the cable, drop the cable!
Aye-aye, sir, aye-aye!
Captain, Captain!

As we live a life of ease (a life of ease)
Everyone of us (everyone of us) has all we need (has all we need)
Sky of blue (sky of blue) and sea of green (sea of green)
In our yellow (in our yellow) submarine (submarine, ah-ha)

We all live in a yellow submarine
Yellow submarine, yellow submarine
We all live in a yellow submarine
Yellow submarine, yellow submarine

We all live in a yellow submarine
Yellow submarine, yellow submarine
We all live in a yellow submarine
Yellow submarine, yellow submarine
",
            ["She Said, She Said"] = @"
She said, ""I know what it's like to be dead.
I know what it is to be sad.""
And she's making me feel like I've never been born

I said, ""Who put all those things in your head?
Things that make me feel that I'm mad.
And you're making me feel like I've never been born.""

She said, ""You don't understand what I said.""
I said, ""No, no, no, you're wrong.
When I was a boy everything was right,
Everything was right.""

I said, ""Even though you know what you know,
I know that I'm ready to leave
'Cause you're making me feel like I've never been born.""

She said, ""You don't understand what I said.""
I said, ""No, no, no, you're wrong.
When I was a boy everything was right,
Everything was right.""

I said, ""Even though you know what you know,
I know that I'm ready to leave
'Cause you're making me feel like I've never been born.""

She said, ""I know what it's like to be dead.
I know what it is to be sad.
I know what it's like to be dead...""
",
            ["Good Day Sunshine"] = @"
Good day sunshine, good day sunshine, good day sunshine

I need to laugh and when the sun is out
I've got something I can laugh about
I feel good in a special way
I'm in love and it's a sunny day

Good day sunshine, good day sunshine, good day sunshine

We take a walk, the sun is shining down
Burns my feet as they touch the ground

Good day sunshine, good day sunshine, good day sunshine

Then we'd lie beneath the shady tree
I love her and she's loving me
She feels good, she knows she's looking fine
I'm so proud to know that she is mine.

Good day sunshine, good day sunshine, good day sunshine
Good day sunshine, good day sunshine, good day sunshine
Good day sunshine, good day sunshine, good day sunshine
Good day...
",
            ["And Your Bird Can Sing"] = @"
You tell me that you've got everything you want
And your bird can sing
But you don't get me, you don't get me

You say you've seen Seven Wonders and your bird is green
But you can't see me, you can't see me

When your prized possessions start to weigh you down
Look in my direction, I'll be 'round, I'll be 'round

When your bird is broken will it bring you down
You may be awoken, I'll be 'round, I'll be 'round

You tell me that you've heard every sound there is
And your bird can swing
But you can't hear me, you can't hear me
",

            ["For No One"] = @"
Your day breaks, your mind aches
You find that all her words of kindness linger on
When she no longer needs you

She wakes up, she makes up
She takes her time and doesn't feel she has to hurry
She no longer needs you

And in her eyes you see nothing
No sign of love behind the tears
Cried for no one
A love that should have lasted years

You want her, you need her
And yet you don't believe her when she says her love is dead
You think she needs you

And in her eyes you see nothing
No sign of love behind the tears
Cried for no one
A love that should have lasted years

You stay home, she goes out
She says that long ago she knew someone but now he's gone
She doesn't need him

Your day breaks, your mind aches
There will be times when all the things she said will fill your head
You won't forget her

And in her eyes you see nothing
No sign of love behind the tears
Cried for no one
A love that should have lasted years
",

            ["Doctor Robert"] = @"
Ring my friend, I said you'd call Doctor Robert
Day or night he'll be there any time at all, Doctor Robert
Doctor Robert, you're a new and better man
He helps you to understand
He does everything he can, Doctor Robert

If you're down he'll pick you up, Doctor Robert
Take a drink from his special cup, Doctor Robert
Doctor Robert, he's a man you must believe
Helping anyone in need
No one can succeed like Doctor Robert

Well, well, well, you're feeling fine
Well, well, well, he'll make you... Doctor Robert

My friend works for the national health, Doctor Robert
Don't pay money just to see yourself with Doctor Robert
Doctor Robert, you're a new and better man
He helps you to understand
He does everything he can, Doctor Robert

Well, well, well, you're feeling fine
Well, well, well, he'll make you... Doctor Robert

Ring my friend, I said you'd call Doctor Robert
Ring my friend, I said you'd call Doctor Robert
",

    ["Doctor Robert"] = @"
Ring my friend, I said you'd call Doctor Robert
Day or night he'll be there any time at all, Doctor Robert
Doctor Robert, you're a new and better man
He helps you to understand
He does everything he can, Doctor Robert

If you're down he'll pick you up, Doctor Robert
Take a drink from his special cup, Doctor Robert
Doctor Robert, he's a man you must believe
Helping anyone in need
No one can succeed like Doctor Robert

Well, well, well, you're feeling fine
Well, well, well, he'll make you... Doctor Robert

My friend works for the national health, Doctor Robert
Don't pay money just to see yourself with Doctor Robert
Doctor Robert, you're a new and better man
He helps you to understand
He does everything he can, Doctor Robert

Well, well, well, you're feeling fine
Well, well, well, he'll make you... Doctor Robert

Ring my friend, I said you'd call Doctor Robert
Ring my friend, I said you'd call Doctor Robert
Doctor Robert",

            ["I Want To Tell You"] = @"
I want to tell you
My head is filled with things to say
When you're here
All those words, they seem to slip away

When I get near you
The games begin to drag me down
It's alright
I'll make you maybe next time around

But if I seem to act unkind
It's only me, it's not my mind
That is confusing things

I want to tell you
I feel hung up but I don't know why
I don't mind
I could wait forever, I've got time

Sometimes I wish I knew you well,
Then I could speak my mind and tell you
Maybe you'd understand

I want to tell you
I feel hung up but I don't know why
I don't mind
I could wait forever, I've got time, I've got time, I've got time
",
            ["Got To Get You Into My Life"] = @"
I was alone, I took a ride
I didn't know what I would find there
Another road where maybe I could see another kind of mind there

Ooh, then I suddenly see you
Ooh, did I tell you I need you
Every single day of my life

You didn't run, you didn't lie
You knew I wanted just to hold you
And had you gone you knew in time we'd meet again
For I had told you

Ooh, you were meant to be near me
Ooh, and I want you to hear me
Say we'll be together every day

Got to get you into my life

What can I do, what can I be
When I'm with you I want to stay there
If I'm true I'll never leave
And if I do I know the way there

Ooh, then I suddenly see you
Ooh, did I tell you I need you
Every single day of my life

Got to get you into my life
Got to get you into my life

I was alone, I took a ride
I didn't know what I would find there
Another road where maybe I could see another kind of mind there

Then suddenly I see you
Did I tell you I need you
Every single day?
",
            ["Tomorrow Never Knows"] = @"
Turn off your mind, relax and float down stream
It is not dying, it is not dying

Lay down all thoughts, surrender to the void
It is shining, it is shining

Yet you may see the meaning of within
It is being, it is being

Love is all and love is everyone
It is knowing, it is knowing...

... that ignorance and hates may mourn the dead
It is believing, it is believing

But listen to the colour of your dreams
It is not living, it is not living

So play the game ""Existence"" to the end...
... Of the beginning, of the beginning
Of the beginning, of the beginning
Of the beginning, of the beginning
Of the beginning, of the beginning
"
        };

        [Fact()]
        public void Embeddings_CreateTextVectors()
        {
            var title = Revolver.Keys.First();
            var client = new OpenAI();
            var ebs = new List<EmbeddingRecord>();
            var itemIndex = 0;
            foreach (var e in Revolver)
            {
                var r = client.Embeddings.Create(e.Value);
                var id = $"{title} - {e.Key}";
                if(itemIndex == 0) 
                    id = $"{title}"; // the first item is a summary of the album

                ebs.Add(r.GenerateEmbeddingRecord(id));
                itemIndex += 1;
            }
            EmbeddingRecord.ToJsonFile(ebs, @"C:\DVT\fAI\src\fAI.Tests\VectorDB\Revolver.json");

            //var input = "Hello world.";
            //var r = client.Embeddings.Create(input);
            //Debug.WriteLine(r.GenerateCSharpCode(ToCSharpName(input)));

            //input = "Yesterday, all my troubles seemed so far away";
            //r = client.Embeddings.Create(input);
            //Debug.WriteLine(r.GenerateCSharpCode(ToCSharpName(input)));

            //input = "Take a sad song and make it better.";
            //r = client.Embeddings.Create(input);
            //Debug.WriteLine(r.GenerateCSharpCode(ToCSharpName(input)));
        }

        private static string ToCSharpName(string input)
        {
            return input.Replace(" ", "").Replace(".","");
        }
    }
}