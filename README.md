# Steal The Cats API
StealTheCatsAPI project. 
This API interacts with the Cats as a Service (CaaS) API to fetch and store cat images and associated metadata into a local database and a file directory.
The system uses ASP.NET Core 8 with Entity Framework Core for data storage, with support for Microsoft SQL Server.Fetches 25 unique cat images from the CaaS API and 
stores them in the local database and downloads them to a file directory.(~\repos\StealTheCatsAPI\StealTheCatsAPI\DownloadedCatImages by Default as provided in appsettings.json)

Includes information such as the image's CatId, Width, Height, and associated Tags (e.g., cat temperament). Duplicate entries are prevented. Retrieves a paginated list of stored cats, with support for specifying the page number and page size 
filtered by a specific tag (e.g., a cat's temperament like "playful").The Pagination is supported with page and pageSize query parameters.

In this project we created a class named AppDbContext that inherits from DbContext in the Entity Framework Core library. It represents the database context for the application and
 is responsible for defining the database schema and managing the interaction with the database.
The AppDbContext class includes two DbSet properties: Cats and Tags, which represent the corresponding database tables for the Cat and Tag entities.
The OnModelCreating method is overridden to configure the entity mappings and define the relationships between the Cat and Tag entities. It uses the ModelBuilder object to define the primary keys, properties, indexes, and constraints for each entity.
Additionally, a many-to-many relationship is established between the Cat and Tag entities using the UsingEntity method, which creates a junction table named "CatTags" to store the relationship between the two entities.
Overall, this code sets up the database context and defines the schema for the Cat and Tag entities, including their properties, relationships, and constraints.
To create our database we must apply a migration with code-first approach by following the bellow steps:
Open Package Manager Console (Tools > NuGet Package Manager > Package Manager Console).

Run the following command:  Add-Migration InitialCreate
InitialCreate is the name of the migration. You can choose any name you prefer.

After creating the migration, you need to apply it to the database to create the tables.
Open Package Manager Console (Tools > NuGet Package Manager > Package Manager Console).

Run the following command: Update-Database

This will apply the migration and create the database using the connection string specified.(in appsettings.json)
The produced script  is the bellow

      CREATE TABLE [Cats] (
          [Id] int NOT NULL IDENTITY,
          [CatId] nvarchar(50) NOT NULL,
          [Width] int NOT NULL,
          [Height] int NOT NULL,
          [Image] nvarchar(max) NOT NULL,
          [Created] datetime2(7) NOT NULL DEFAULT (GETUTCDATE()),
          CONSTRAINT [PK_Cats] PRIMARY KEY ([Id]),
          CONSTRAINT [CK_CatEntity_Width_Height] CHECK ([Width] >= 100 AND [Width] <= 9000 AND [Height] >= 100 AND [Height] <= 9000)
      );
      CREATE TABLE [Tags] (
          [Id] int NOT NULL IDENTITY,
          [Name] nvarchar(100) NOT NULL,
          [Created] datetime2(7) NOT NULL DEFAULT (GETUTCDATE()),
          CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
      );

      CREATE TABLE [CatTags] (
          [CatsId] int NOT NULL,
          [TagsId] int NOT NULL,
          CONSTRAINT [PK_CatTags] PRIMARY KEY ([CatsId], [TagsId]),
          CONSTRAINT [FK_CatTags_Cats_CatsId] FOREIGN KEY ([CatsId]) REFERENCES [Cats] ([Id]) ON DELETE CASCADE,
          CONSTRAINT [FK_CatTags_Tags_TagsId] FOREIGN KEY ([TagsId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
      );

      CREATE UNIQUE INDEX [IX_Cat_CatId] ON [Cats] ([CatId]);

      CREATE INDEX [IX_CatTags_TagsId] ON [CatTags] ([TagsId]);

      CREATE UNIQUE INDEX [IX_Tag_Name_Unique] ON [Tags] ([Name]);
	  
	  
Note: if you want to create the database in your machine please delete the Migrations File in StealTheCatsAPI.Application 
and run the above commands at Package Manager Console by adding as Default Project the StealTheCatsAPI.Application 