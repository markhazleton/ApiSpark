using WebSpark.Recipe.Models;

namespace ApiSpark.Api.Features.Recipe;

public static class RecipeEndpoints
{
    public static RouteGroupBuilder MapPublicRecipeApi(this RouteGroupBuilder group)
    {
        group.MapGet("/recipes", (RecipeService svc) =>
            Results.Ok(svc.GetApprovedRecipes()))
            .WithName("GetApprovedRecipes")
            .WithTags("Recipes")
            .AllowAnonymous();

        group.MapGet("/recipes/{id:int}", (int id, RecipeService svc) =>
        {
            var recipe = svc.GetRecipeById(id);
            return recipe is null
                ? Results.NotFound(new { message = $"Recipe {id} not found." })
                : Results.Ok(recipe);
        })
        .WithName("GetRecipeById")
        .WithTags("Recipes")
        .AllowAnonymous();

        group.MapGet("/recipes/categories", (RecipeService svc) =>
            Results.Ok(svc.GetCategories()))
            .WithName("GetRecipeCategories")
            .WithTags("Recipes")
            .AllowAnonymous();

        return group;
    }

    public static RouteGroupBuilder MapPublishRecipeApi(this RouteGroupBuilder group)
    {
        group.MapPost("/recipes", (RecipeModel model, RecipeService svc) =>
        {
            var created = svc.CreateRecipe(model);
            return Results.Created($"/api/public/recipes/{created.Id}", created);
        })
        .WithName("CreateRecipe")
        .WithTags("Recipes");

        group.MapPut("/recipes/{id:int}", (int id, RecipeModel model, RecipeService svc) =>
        {
            var updated = svc.UpdateRecipe(id, model);
            return updated is null
                ? Results.NotFound(new { message = $"Recipe {id} not found." })
                : Results.Ok(updated);
        })
        .WithName("UpdateRecipe")
        .WithTags("Recipes");

        group.MapDelete("/recipes/{id:int}", (int id, RecipeService svc) =>
            svc.DeleteRecipe(id)
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Recipe {id} not found." }))
            .WithName("DeleteRecipe")
            .WithTags("Recipes");

        group.MapPost("/recipes/categories", (RecipeCategoryModel model, RecipeService svc) =>
        {
            var created = svc.CreateCategory(model);
            return Results.Created($"/api/public/recipes/categories/{created.Id}", created);
        })
        .WithName("CreateRecipeCategory")
        .WithTags("Recipes");

        group.MapPut("/recipes/categories/{id:int}", (int id, RecipeCategoryModel model, RecipeService svc) =>
        {
            var updated = svc.UpdateCategory(id, model);
            return updated is null
                ? Results.NotFound(new { message = $"Category {id} not found." })
                : Results.Ok(updated);
        })
        .WithName("UpdateRecipeCategory")
        .WithTags("Recipes");

        group.MapDelete("/recipes/categories/{id:int}", (int id, RecipeService svc) =>
            svc.DeleteCategory(id)
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Category {id} not found." }))
            .WithName("DeleteRecipeCategory")
            .WithTags("Recipes");

        return group;
    }
}
