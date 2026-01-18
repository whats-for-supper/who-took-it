# Who Took It?
Who took the free drinks in hack and roll!

Notice how quickly the free drinks fridge is replenishing? Yeah it's the same big backs coming back to hoard all the milos.

This web application detects whether a person has retrieved a drink from the fridge through the camera and sounds an alarm if it detects you appeared more than once.

**Link to project:** [url once deployed]

## About
Using computer vision, it detects whether a face is in the frame. If yes, it checks whether the face is new. If yes, it adds you to a database. If no, we've found a thief! Hence, the loud blaring alarm will sound.

**Tech Stack:** 
Frontend: HTMLCSS, JavaScript, Django
Backend: Supabase, .NET, C#
Image Recognition: InsightFace, Numpy

## How we built it
We built it using InsightFace. The frontend stack was made with HTML, CSS, and JavaScript, created as a Django project. The database was stored in Supabase as a pgvector embedding, so personal information and facial data is stored in a secure way. We deployed it using AWS Elastic Beanstalk.

## Acknowledgements
Font used: [Enchanted Land](https://www.dafont.com/enchanted-land.font)
