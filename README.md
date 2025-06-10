ONLINE GRUPPENS C2C WEBSITE : LAPSWAP


Written and tested in Visual Studio 2022. 

Requirements to run the application:
Visual Studio 2022
A Database (We ran ours at Neon.Tech) - Remember to use prober linking with appsetting.json to your database.
  - That consists of these three tables:
          1: listing_images [id serial]   [listing_id integer]   [image_path varchar(255)]  [listing]
          2: listings  [id serial] [brand varchar(255)] [model varchar(255)] [gpu varchar(255)] [cpu varchar(255)] [ram integer] [storage integer] [osvarchar(255)] [price numeric (10, 2)]                         [screen_size varchar(255)] [condition varchar(255)] [title text] [description text] [phone varchar(255)] [location varchar(255)]                                                             [created_utc  timestamp with time zone]  [user_id integer]  [listing_images] [users]
          3: users [id serial] [name varchar(255)] [email varchar(255)] [password varchar(255)] [phone varchar(255)] [address varchar(255)] [city varchar(255)] [zip code varchar(255)]                         [listings]


