USE [DEV]
GO

/****** Object:  StoredProcedure [dbo].[GetTags]    Script Date: 2023-09-12 9:15:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create procedure [dbo].[GetTags]
@recipeID int
as 
begin
	Select ID, Tag from RecipeTags where RecipeID = @recipeID
end
GO


Create procedure [dbo].[GetSteps]
@recipeID int
as 
begin
	Select ID, Step from RecipeSteps where RecipeID = @recipeID
end
GO

Create procedure [dbo].[GetRecipes]
as 
begin
	Select * from Recipes
end
GO

Create procedure [dbo].[GetIngredients]
@recipeID int
as 
begin
	Select ID, IngredientDesc, Quantity, QuantityDesc  from RecipeIngredients where RecipeID = @recipeID
end
GO

CREATE procedure [dbo].[CreateTag]
@recipeID int,
@tag varchar(255)
as 
begin
	insert into RecipeTags values (@recipeID, @tag);
	select @@identity as id
end
GO

CREATE procedure [dbo].[CreateStep]
@recipeID int,
@Step varchar(255)
as 
begin
	insert into RecipeSteps values (@recipeID, @Step);
	select @@identity as id
end
GO

CREATE procedure [dbo].[CreateRecipe]
@name varchar(255),
@desc varchar(255)
as 
begin
	insert into Recipes values (@name, @desc);
	select @@IDENTITY as id
end
GO

CREATE procedure [dbo].[CreateIngredient]
@recipeID int,
@name varchar(255),
@amount int,
@amountDesc varchar(255)
as 
begin
	insert into RecipeIngredients values (@recipeID, @name, @amount, @amountDesc);
	select @@identity as id
end
GO

