# EvePublicStanding
Public standing service. Discontinued.

Wanted to see if there was any interest in a service where you could search for characters and see how well liked, or not liked that character is. Basically the idea is to gather the standings that people have set for other characters and generate a rating which basically tells you how well liked that character is.

The scale is from -10 to 10 and its calculated by taking the current standing saved in the database, adding the new standing and then dividing by the amount of submissions for that character. I'm pretty sure this will keep the saved standing within the scale (correct me if I'm wrong, I'm no math wiz). To add your contacts just submit your API details on the form. NOTE that you may only submit once per week, per device, so choose which character wisely. **No API keys are stored!** Your API is used to grab the contacts of your character right away. In fact, there is no way to link you to any character in the database.

IMPORTANT: When you create an API key only select Contacts, the access mask is 16. Make sure to select a single character and not ALL.

Please let me know your thoughts, thanks!
Reddit post:  [https://redd.it/3qfb3v](https://redd.it/3qfb3v)

**FAQ**

> Isn't this what evewho is?

*  No, evewho is all public character information. This uses players' actual standings towards each other to provide an overall standing within eve.

> I don't think people would want their contacts information public.

*  When you submit your api key, the system does grab the standings from your contacts. But only the numbers and adds it to the database and calculates the new standing on the -10 to 10 scale. There is no information linking who's character submitted what.

> I'd imagine people would just make a bunch of alts to make themselves look better.

*  I have limited submissions from the same device for a certain time frame to discourage abuse, but nothing is perfect and a few erroneous submissions would be just a drop in the bucket anyway.

> Why bother with this?

*  I have always found Eve to be problematic in individual accountability because of the games design and you can never trust anyone because of this. Of course I know I can't solve this problem but I feel like I can help.  Eve has always had standings, sure. But they have never really meant anything before, and I wanted to change that. This system encourages you to set standings, since before the only benefit was seeing blue or red in chat and overview. Now you can actually affect other characters' reputations if you wish.
