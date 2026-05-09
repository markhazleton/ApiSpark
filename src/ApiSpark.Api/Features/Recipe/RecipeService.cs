using WebSpark.Recipe.Interfaces;
using WebSpark.Recipe.Models;

namespace ApiSpark.Api.Features.Recipe;

public class RecipeService(IRecipeService recipeService)
{
    public IReadOnlyList<RecipeModel> GetApprovedRecipes()
        => recipeService.Get().Where(r => r.IsApproved).ToList();

    public RecipeModel? GetRecipeById(int id)
    {
        var model = recipeService.Get(id);
        return model.Id == 0 ? null : model;
    }

    public IReadOnlyList<RecipeCategoryModel> GetCategories()
        => recipeService.GetRecipeCategoryList();

    public RecipeModel CreateRecipe(RecipeModel model)
        => recipeService.Save(model);

    public RecipeModel? UpdateRecipe(int id, RecipeModel model)
    {
        var existing = recipeService.Get(id);
        if (existing.Id == 0) return null;
        model.Id = id;
        return recipeService.Save(model);
    }

    public bool DeleteRecipe(int id)
        => recipeService.Delete(id);

    public RecipeCategoryModel CreateCategory(RecipeCategoryModel model)
        => recipeService.Save(model);

    public RecipeCategoryModel? UpdateCategory(int id, RecipeCategoryModel model)
    {
        var existing = recipeService.GetRecipeCategoryById(id);
        if (existing.Id == 0) return null;
        model.Id = id;
        return recipeService.Save(model);
    }

    public bool DeleteCategory(int id)
    {
        var category = recipeService.GetRecipeCategoryById(id);
        if (category.Id == 0) return false;
        return recipeService.Delete(category);
    }
}
