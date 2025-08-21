USE [DEV]
GO

Create or alter procedure [dbo].[GetTags]
@recipeID int
as 
begin
	Select ID, Tag from RecipeTags where RecipeID = @recipeID
end
GO

Create or alter procedure [dbo].[GetSteps]
@recipeID int
as 
begin
	Select ID, Step, SubText from RecipeSteps where RecipeID = @recipeID
end
GO

Create or alter procedure [dbo].[GetRecipes]
as 
begin
	Select * from Recipes
end
GO

create or alter procedure [dbo].[GetRecipeIngredients]
@recipeID int
as 
begin
	Select a.IngID, a.RecipeID, b.IngredientName, a.IngredientNameModifier, a.Quantity, a.QuantityDesc  from 
	RecipeIngredients a 
	inner join Ingredient b on a.IngID = b.ID
	where a.RecipeID = @recipeID
end
GO

Create or alter procedure [dbo].[CreateTag]
@recipeID int,
@tag varchar(255)
as 
begin
	insert into RecipeTags values (@recipeID, @tag);
	select @@identity as id
end
GO

Create or alter procedure [dbo].[CreateStep]
@recipeID int,
@Step varchar(max),
@SubText varchar(max)
as 
begin
	insert into RecipeSteps values (@recipeID, @Step, @SubText);
	select @@identity as id
end
GO

Create or alter procedure [dbo].[CreateRecipe]
@name varchar(255),
@desc varchar(255)
as 
begin
	insert into Recipes values (@name, @desc);
	select @@IDENTITY as id
end
GO

create or alter procedure [dbo].[CreateRecipeIngredient]
@ingID int,
@recipeID int,
@name varchar(50),
@amount int,
@amountDesc varchar(255)
as 
begin
	insert into RecipeIngredients values (@ingID, @recipeID, @name, @amount, @amountDesc);
end
GO

create or alter procedure dbo.CreateIngredient
@name varchar(40)
as 
begin
	insert into Ingredient values(@name);
	select @@IDENTITY as id
end
go

create or alter procedure dbo.GetAllIngredients
as 
begin
select ID, IngredientName from Ingredient;
end
go

create or alter procedure dbo.GetFilteredIngredients
@search varchar(40)
as
begin
	select ID, IngredientName from Ingredient
	where IngredientName like '%'+@search+'%';
end
go

create or alter procedure dbo.DeleteTag
@id int
as
begin
	Delete from RecipeTags where id = @id
end;
