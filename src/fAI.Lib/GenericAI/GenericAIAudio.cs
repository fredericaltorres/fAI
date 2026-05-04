using System;
using System.Collections.Generic;

namespace fAI
{
    public partial class GenericAIAudio : HttpBase
    {
        public GenericAIAudio(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        public string Create(GenericAIAudioProvider provider, string input, string voiceName, string mp3FileName = null)
        {
            OpenAI.Trace(new { input, voiceName }, this);
            switch (provider)
            {
                case GenericAIAudioProvider.HUME_AI:
                    var humeClient = new HumeAI(apiKey: base._key);
                    return humeClient.Audio.Speech.Create(input, voiceName, mp3FileName);

                case GenericAIAudioProvider.OPEN_AI:
                    var openAIClient = new OpenAI(apiKey: base._key);
                    return openAIClient.Audio.Speech.Create(input, voiceName, mp3FileName);
                default:
                    throw new Exception($"Audio provider {provider} not supported.");
            }
        }

        public Dictionary<string, string> GetVoices(GenericAIAudioProvider provider)
        {
            OpenAI.Trace( new { }, this);
            var r = new Dictionary<string, string>();
            var TMP_DIC = new Dictionary<string, string>();
            switch (provider)
            {
                case GenericAIAudioProvider.HUME_AI:
                    {
                        TMP_DIC["a7ecc00a-6fc0-4546-8126-e12cfd8de3bf"] = "Alice Bennett";
                        TMP_DIC["43e411b3-b2cc-40da-b742-4abf0e3557b2"] = "American Lead Actress";
                        TMP_DIC["4652d91b-edaf-42c5-abd4-904009422de3"] = "Articulate ASMR British Narrator";
                        //TMP_DIC["1a655f88-551e-4633-a502-4c0d668168e8"] = "Aunt Tea";
                        //TMP_DIC["5bb7de05-c8fe-426a-8fcc-ba4fc4ce9f9c"] = "Ava Song";
                        //TMP_DIC["2a7b176a-ca45-4ff8-8a65-56f873a5fdc7"] = "Awe Inspired Guy";
                        //TMP_DIC["95f9baac-4512-4edb-a73e-2070784ccc2f"] = "Big Dicky";
                        //TMP_DIC["445d65ed-a87f-4140-9820-daf6d4f0a200"] = "Booming American Narrator";
                        TMP_DIC["84aaf67b-dcab-409d-9f48-8c4aa7abbb24"] = "Booming British Narrator";
                        TMP_DIC["15f594d3-0683-4585-b799-ce12e939a0e2"] = "Brooding Intellectual Man";
                        //TMP_DIC["a3d1e23c-403e-423b-aeae-c568ad0bccba"] = "California Frat Bro";
                        //TMP_DIC["b89de4b1-3df6-4e4f-a054-9aed4351092d"] = "Campfire Narrator";
                        //TMP_DIC["97fe9008-8584-4d56-8453-bd8c7ead3663"] = "Caring Mother";
                        //TMP_DIC["33045fd9-8010-43f6-b6b0-da3fbf326c29"] = "Casual Podcast Host";
                        TMP_DIC["4b305eef-58eb-455c-a154-be40c3129d0b"] = "Charismatic Politician Man"; // OBAMA
                        //TMP_DIC["3f636d17-44c7-4872-93d1-0c8f51c916a3"] = "Charming Cowgirl";
                        //TMP_DIC["dd39f331-a857-4c20-908a-1f0c56b0a79b"] = "Cheerful Canadian";
                        //TMP_DIC["d8de3d55-9fcc-4aad-ac93-131141602717"] = "Cheerful Irishman";
                        TMP_DIC["fcd2297b-44dd-4115-97af-a13297afb8cb"] = "Classical Film Actor";
                        TMP_DIC["faf64860-5d8c-44b2-9fc3-88717d307ce8"] = "Classical Film Actress";
                        TMP_DIC["a2cff8f5-1550-4597-9139-63ac4a468b48"] = "Colorful Fashion Influencer";
                        //TMP_DIC["d8ab67c6-953d-4bd8-9370-8fa53a0f1453"] = "Colton Rivers";
                        TMP_DIC["99d2cb9c-9011-4ead-8734-641656d3df66"] = "Comforting Male Conversationalist";
                        //TMP_DIC["39e3bc67-3cac-477e-910c-0bb91f3191a8"] = "Comical Vampire";
                        TMP_DIC["d1248151-8613-41c1-b524-4ce242b02090"] = "Conversational English Guy";
                        TMP_DIC["f3f69312-095c-4ec3-8e50-6961c676e898"] = "Cool Journalist";
                        TMP_DIC["9c5a3d53-4a8c-4fa2-adad-8e61a830d0e8"] = "Deep Male Conversational Voice";
                        //TMP_DIC["d6fd5cc2-53e6-4e80-ba83-93972682386a"] = "Demure Conversationalist";
                        //TMP_DIC["f042c0be-b7cc-4a59-bea2-65f23e12c710"] = "Donovan Sinclair";
                        //TMP_DIC["c11052f5-96df-4c0e-9bba-07e0ad19c4b3"] = "Dramatic Movie Trailer Narrator";
                        //TMP_DIC["44cb3a51-07b4-4934-83fd-9d9e8d363ce6"] = "Dungeon Master";
                        //TMP_DIC["21289f74-417c-422c-be9f-b8f84ee07d44"] = "Ellie";
                        TMP_DIC["5add9038-28df-40a6-900c-2f736d008ab3"] = "English Casual Conversationalist";
                        TMP_DIC["a3cc3538-c557-45e0-ada0-b022937d51c1"] = "English Children's Book Narrator";
                        //TMP_DIC["36e7572e-f5b2-477e-ae61-4400dbeaa034"] = "Excitable British Naturalist";

                        //TMP_DIC["6b530c02-5a80-4e60-bb68-f2c171c5029f"] = "Expressive Girl";
                        //TMP_DIC["661ab31e-c4d6-4a16-952a-b5806a9b4ad1"] = "Fastidious Robo-Butler";
                        //TMP_DIC["b270ea4b-d169-4d1b-83c4-2e816e947522"] = "Female Meditation Guide";
                        //TMP_DIC["f8a649d1-0fb6-4168-a1e2-e9ec41937c55"] = "French Chef";
                        //TMP_DIC["35fc083c-a935-40cb-8cfe-805e76009041"] = "Friendly Kiwi Girl";
                        //TMP_DIC["1200f557-fdf2-4960-8954-6a0591513051"] = "Friendly Kiwi Guy";
                        //TMP_DIC["9c2fa3e6-7bbf-4e71-8838-3d28f26cc269"] = "Friendly Troll";
                        //TMP_DIC["06646694-ba2a-4bca-ae3c-71d79c6b04a3"] = "Geraldine Wallace";
                        //TMP_DIC["2c0e2c10-ac19-4aac-93d0-29c385d7364e"] = "Ghost With Unfinished Business";
                        //TMP_DIC["9388af0d-4d33-4cdf-8b0a-f003b6cf9455"] = "Grizzled New Yorker";
                        //TMP_DIC["7e65fb10-8ef6-4faf-a111-875102d51b25"] = "Groovy Guy";
                        //TMP_DIC["da0a369e-7799-40d3-b5c8-3015f198ef57"] = "Highly Reactive Guy";
                        //TMP_DIC["96ee3964-5f3f-4a5a-be09-393e833aaf0e"] = "Imani Carter";
                        //TMP_DIC["710e69a0-ea28-4165-8b2f-c3453686b595"] = "Indian Actor";
                        //TMP_DIC["1fe215ab-513c-4fc8-9233-fa64b65073ab"] = "Indian Actress";
                        //TMP_DIC["8c7d03bd-20d4-40e9-aca1-0469af8ae450"] = "Inspiring Man";
                        //TMP_DIC["de314c2f-0013-4e7c-92d0-f60ca114ff5b"] = "Inspiring Older Guy";
                        //TMP_DIC["b201d214-914c-4d0a-b8e4-54adfc14a0dd"] = "Inspiring Woman";
                        //TMP_DIC["f60ecf9e-ff1e-4bae-9206-dba7c653a69e"] = "Ito";
                        //TMP_DIC["81c3cff8-5ac2-4411-be82-81e92fa73838"] = "James the Archivist";
                        //TMP_DIC["59cfc7ab-e945-43de-ad1a-471daa379c67"] = "Kora";
                        //TMP_DIC["c7aa10be-57c1-4647-9306-7ac48dde3536"] = "Lady Elizabeth";
                        //TMP_DIC["5cad536a-3013-4f01-8390-d6d405d266a9"] = "Literature Professor";
                        //TMP_DIC["cb1a4fae-dad5-4729-bd73-a43f570b9117"] = "Live Comedian";
                        //TMP_DIC["6b69954e-6c9a-4ade-ad03-f73e116e1eae"] = "Male Australian Naturalist";
                        //TMP_DIC["9e068547-5ba4-4c8e-8e03-69282a008f04"] = "Male English Actor";
                        //TMP_DIC["01854384-4e4e-48d4-90d1-b22f760a58b5"] = "Male Podcaster";
                        //TMP_DIC["82a76fb8-3524-4e87-9265-9795c8e4ede6"] = "Male Protagonist";
                        //TMP_DIC["a1e5674e-3ef1-4394-913c-d4cd70b96801"] = "Medieval Peasant Man";
                        //TMP_DIC["a6984261-4b9f-492c-a5fd-9ddbe14039c4"] = "Medieval Peasant Woman";
                        //TMP_DIC["0366be28-522b-432b-b346-119d5f21c3f2"] = "Medieval Town Crier";
                        //TMP_DIC["375cd12e-8216-4c6d-8d79-5dfd56fe19f5"] = "Mrs. Pembroke";
                        //TMP_DIC["f898a92e-685f-43fa-985b-a46920f0650b"] = "Mysterious Woman";
                        //TMP_DIC["203d3e34-e50b-48ab-a782-835fa39c36c6"] = "Nasal Podcast Host";
                        //TMP_DIC["176a55b1-4468-4736-8878-db82729667c1"] = "Nature Documentary Narrator";
                        //TMP_DIC["28441f64-64a0-4df4-9bd2-478850ee5fac"] = "New York Comedian Guy";
                        //TMP_DIC["88e0c3f6-6f99-4e37-a3f4-a053869d6bd4"] = "Old School Radio Announcer";
                        //TMP_DIC["27019e54-59c4-4400-a51c-7e5fd2142029"] = "Old-Timey English Priest";
                        //TMP_DIC["89989d92-1de8-4e5d-97e4-23cd363e9788"] = "Opinionated Guy";
                        //TMP_DIC["2bc0fca6-d591-4855-abef-ec048a385e8f"] = "Pirate Captain";
                        //TMP_DIC["8b9ed861-3382-455d-9c03-a40671943d6b"] = "Rajesh";
                        //TMP_DIC["aeaaf1f8-fe31-49ae-893d-c744e5207bc2"] = "Relaxing ASMR Woman";
                        //TMP_DIC["55813df9-fdbb-4ec2-a539-1b91fd750ca6"] = "Sad Old British Man";
                        //TMP_DIC["a48360cb-14f3-460c-93f2-b38deb45400b"] = "Scottish Guy";
                        //TMP_DIC["f49f18fc-4fe9-4e78-9338-0960d44b93f9"] = "Scottish Scott";
                        //TMP_DIC["27e8dd8b-7e7c-4c1f-bfb2-c1b016487343"] = "Seasoned Midwestern Actress";
                        //TMP_DIC["522fc367-961f-4817-8929-81f433b4afe9"] = "Sebastian Lockwood";
                        //TMP_DIC["71de875d-bc14-4ed5-87da-8584ba4ea247"] = "Serene Assistant";
                        //TMP_DIC["921ece15-27c1-4028-b6ac-82d2d93f65c4"] = "Sir Spandrel";
                        //TMP_DIC["5bbc32c1-a1f6-44e8-bedb-9870f23619e2"] = "Sitcom Girl";
                        //TMP_DIC["c5be03fa-09cc-4fc3-8852-7f5a32b5606c"] = "Sitcom Guy";
                        //TMP_DIC["b152864b-6720-496a-9d18-eaadb31516ee"] = "Soft Male Conversationalist";
                        //TMP_DIC["f4703974-b6a1-45c8-ac72-72ea06e3dd43"] = "Steve Frisch";
                        //TMP_DIC["7f633ac4-8181-4e0d-99e1-11a4ef033691"] = "Terrence Bentley";
                        //TMP_DIC["ebba4902-69de-4e01-9846-d8feba5a1a3f"] = "TikTok Fashion Influencer";
                        //TMP_DIC["6a42908a-f332-4c39-a93a-98c7d6017f12"] = "Tough Guy";
                        //TMP_DIC["41745808-6790-4be4-83db-363e20db7e71"] = "Trevor";
                        //TMP_DIC["e722344d-e09a-489d-9f10-b67a28edd35a"] = "Turtle Guru";
                        //TMP_DIC["2ca55181-9d21-43b3-9e6e-0cb24a669e6c"] = "Unserious Movie Trailer Narrator";
                        //TMP_DIC["a5b1def0-16a5-4fcc-bfa8-2a0de2e35d93"] = "Unserious TV Host";
                        //TMP_DIC["ee96fb5f-ec1a-4f41-a9ba-6d119e64c8fd"] = "Vince Douglas";
                        //TMP_DIC["8a7dd58c-0cda-4073-9ce6-654184695e99"] = "Warm American Female";
                        //TMP_DIC["a623d3ed-612c-413b-b09f-e0a379a317f0"] = "Warm Female Assistant Voice";
                        //TMP_DIC["f33a4a30-d1bc-450e-9174-082dec6aa571"] = "Warm Welsh Lady";
                        //TMP_DIC["4a9c32ab-e7b5-4439-a7be-54cca5ad9c07"] = "Welsh Folk Storyteller";
                        //TMP_DIC["e7024495-03e7-4e8b-8b22-27c54ee25ffa"] = "Wise Wizard";
                        //TMP_DIC["dc6f3593-4ae1-46eb-9121-b872ad7ba1a0"] = "Wrestling Announcer";
                        //TMP_DIC["2ae087da-ab61-4455-b095-4e926f0e75a2"] = "Yorkshire Chap";

                        return TMP_DIC;
                    }

                case GenericAIAudioProvider.OPEN_AI:

                    OpenAISpeech.VoicesAsString.ForEach(v => TMP_DIC[v] = v);
                    return TMP_DIC;
                default:
                    throw new Exception($"Audio provider {provider} not supported.");
            }
        }
    }
}