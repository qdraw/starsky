---
slug: the-7-things-i-missed-when-managing-my-photo-collection
title: The 7 things I missed when managing my photo collection
authors: dion
tags: [photo mangement, welcome]
---



I really enjoy going out and taking pictures. But when I get home, it's time to properly organize all those photos. That task is typically something we put off, but it is nice to share the photos and experiences – for example on this blog and various social media channels where I regularly post something. So it was high time to optimize the time-consuming process of photo management. In this article, based on my own experiences, I will discuss the solutions I found for the 7 most common problems in effectively managing a photo collection.

1\. Quick search, quick find
----------------------------

Finding that one photo you're looking for can be quite difficult. That's why finding what you've been looking for quickly and easily is a great way to get a lot more done in a shorter amount of time. For example, Spotlight (search) works great on Mac OS, although it is not integrated in the mobile version. When you place a number of keywords in the meta data field in an image, you can search for them later. But also by date, description and information that is stored in the camera. So quick search, quick find ![Snel zoeken, snel vinden](https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/01_video_search_cloud_starsky_v050.gif)


**Press read more to continue... **

<!--truncate-->


2\. Switch from desktop to mobile view
--------------------------------------



Switching seamlessly between your laptop, iPad and phone often causes problems. You have to wait a long time, that one folder is not loading or is not accessible. Quickly showing something to someone is still quite a hassle. That's why I thought about the best way to ensure that you can quickly and easily switch between the desktop and mobile application, without sacrificing essential functionalities. Looking up when something happened or conjuring up the photos of that great holiday becomes a lot easier.

![Demo site on mobile](https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/500/02_starsky_v052_kl.jpg)



3\. Prepare for publication and add a watermark
-----------------------------------------------

Before making a blog or social media post, we first make a selection of the photos we have taken. But how do you get them in the right format? That is often a task that you have to wait until you get home and need a computer with the right software. In my own management program I give different photos that belong together a selection color. This way I can easily make a selection of the photos I want to publish. Based on these selections I do an automatic publication action. This places a watermark in the images. I myself use a number of options that I have defined in advance through settings. By means of configuration it is also possible to generate html with meta information also added so that search engines can find this content faster. Another option is to create a number of reduced images for Instagram, for example. 

![Publication tool](https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/03_video_starsky_v050.gif)

4\. Smart addition of geolocation data
--------------------------------------

Mobile phones accurately track the location of a photo. However, many cameras don't do this yet. To avoid having to manually add location data to each photo, I use a sports app on my phone. It keeps track of my location. I then export this information from the sports app, after which the photo management application automatically brings it all together. Then I put the gpx file containing all the locations and times in the same folder and run a script that automates this integration. In this way you can link the different information sources together. It is important that the camera is exactly on time so that the location at that time matches the location in the sports app. Supplement GPX with data, add location data with 1 click 

![Supplement GPX with data, add location data with 1 click](https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/04_video_starsky_v052.gif)

5\. Import photos and videos with minimal manual actions
--------------------------------------------------------

Transferring the photos and videos from your camera to your photo library is not complicated, but often a manual task. Yet this is easy to automate. You connect your SD card or USB cable and the computer does the rest of the process independently. If your photo library has more structure, for example by organizing the photos by date, you can also configure this in the settings. For now use a background script that is automatically triggered when an SD card is connected and then the images on the memory card are automatically placed in the right place. Import photos and videos with minimal manual actions 

![Import photos and videos with minimal manual actions](https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/05_video_starsky_v052.gif)

6\. Without connection to the internet
--------------------------------------

In some places the connection is not yet that good to send gigabytes of information over the line. A cloud service is not the most convenient solution at such a time.[

![Raspberry Pi with the application running](https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/500/06_20200516_144415_d_e_starsky_v040_kl.jpg)


7\. Control your own data and switch to other software without any problems
---------------------------------------------------------------------------

Data is the new gold and currently the big tech companies are mainly benefiting from it. The solution lies in a more transparent system that returns the power over data to the user. There are hidden fields in every photo where information can be stored. All information about a specific photo that you enter into the photo management tool will end up in the photo. That includes labels, description and location information. When publishing, you can choose not to include this information in the copy. The tool uses standards, all meta information contained in the image is also written into the image itself. So you can switch at any time, all data you have inserted will be saved in the photo.[

![Meta data in sync](https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/500/07_exiftool_starsky_v042_kl.jpg)


Conclusion
----------

As a solution to these 7 problems [I developed a server and desktop application.](https://docs.qdraw.nl/download) My goal was to optimize the time-consuming process of organizing photos – and I also learned a lot while making this application. The app is still in beta stage. [The source code of the project is available online for free on GitHub](https://docs.qdraw.nl/download) and there is a demo environment so that this project can contribute to making your life just that little bit easier. There is also a version available that you can run immediately. For Mac OS users, the application will cause an error as the app has no official Apple certificates. You can manually grant this access in the settings of your computer. By right-clicking open button, clicking past the warnings Try it yourself on a demo environment? Which can! Go to [demo.qdraw.nl](https://demo.qdraw.nl) to test the app