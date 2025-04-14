
internal class DictionaryNode
{
    private readonly DictionaryNode[] links = new DictionaryNode[256];     // TC : O(1)
    private bool isEndOfWord;

    public bool ContainsLetter(char letter)           // TC : O(1)
    {
        return links[letter] != null;
    }

    public void Store(char letter, DictionaryNode node)            // TC : O(1)
    {
        links[letter] = node;
    }

    public DictionaryNode Get(char letter)                       // TC : O(1)
    {
        return links[letter];
    }

    public void SetEnd()                                        // TC : O(1)
    {
        isEndOfWord = true;
    }

    public Boolean IsEnd()                                     // TC : O(1)
    {
        return isEndOfWord;
    }

    public Dictionary<char, DictionaryNode> GetChildren()            // TC: O(256) = O(1)
    {
        var children = new Dictionary<char, DictionaryNode>();
        for (int i = 0; i < links.Length; i++)
        {
            if (links[i] != null)
            {
                children.Add((char)(i), links[i]);
            }
        }
        return children;
    }

}

internal class DictionaryTrie
{
    private readonly DictionaryNode root;

    public DictionaryTrie()
    {
        root = new DictionaryNode();
    }

    public void Insert(string word)            // TC: O(L), where L = length of the word
    {
        DictionaryNode node = root;

        foreach (char letter in word)
        {
            if (!node.ContainsLetter(letter))
            {
                node.Store(letter, new DictionaryNode());
            }
            node = node.Get(letter);
        }
        node.SetEnd();
    }

    public bool Search(string word)                   // TC: O(L), where L = length of the word
    {
        DictionaryNode node = root;
        foreach (char letter in word)
        {
            if (!node.ContainsLetter(letter))
            {
                return false;
            }
            node = node.Get(letter);
        }
        return node.IsEnd();
    }

    public bool StartsWith(string prefix)                  // TC: O(P), where P = length of prefix
    {
        DictionaryNode node = root;
        foreach (char letter in prefix)
        {
            if (!node.ContainsLetter(letter))
            {
                return false;
            }
            node = node.Get(letter);
        }
        return true;
    }

    public List<string> SuggestWords(string inputWord, int maxSuggestions)
    {
        var prefixNode = GetNodeForPrefix(inputWord);             // TC: O(P), where P = length of prefix

        List<string> allSuggestedWords = new List<string>();

        if (prefixNode != null)
        {
            // Only collect from the subtree under this prefix node
            CollectWords(prefixNode, inputWord, allSuggestedWords);

            if (allSuggestedWords.Count > 0)
            {
                return allSuggestedWords.Take(maxSuggestions).ToList();
            }
        }

        List<string> allWords = new List<string>();
        CollectWords(root, "", allWords);                    // TC: O(N × L), N = total words, L = avg word length

        return allWords
            .OrderBy(word => LevenshteinDistance(inputWord, word))     // TC: O(N × M × L), M = input length, L = word length
            .Take(maxSuggestions)
            .ToList();
    }

    private void CollectWords(DictionaryNode dictionaryNode, string current, List<string> result)
    {
        if (dictionaryNode.IsEnd())                   // TC: O(1)
        {
            result.Add(current);                     // TC: O(1)
        }

        foreach (var child in dictionaryNode.GetChildren())
        {
            CollectWords(child.Value, current + child.Key, result);       // recursive depth = O(L)
        }
    }

    private DictionaryNode GetNodeForPrefix(string prefix)    // TC: O(P), where P = length of prefix word
    {
        DictionaryNode node = root;
        foreach (char letter in prefix)
        {
            if (!node.ContainsLetter(letter))
            {
                return null;
            }
            node = node.Get(letter);
        }
        return node;
    }


    private int LevenshteinDistance(string a, string b)          // TC: O(M × L), M= length of a, L = length of b
    {
        int[,] dp = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost
                );
            }
        }
        return dp[a.Length, b.Length];
    }
}

public class Dictionary()
{
    public static void Main(string[] args)
    {
        DictionaryTrie dictionaryTrie = new DictionaryTrie();

        string projectDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        string filePath = Path.Combine(projectDir, "list.txt");
        if (File.Exists(filePath))             // TC: O(1)
        {
            string[] words = File.ReadAllLines(filePath);    // TC: O(N), N = number of lines

            foreach (string word in words)           // TC: O(N × L), N = number of lines, L = length of word
            {
                dictionaryTrie.Insert(word);
            }
        }
        else
        {
            Console.WriteLine("File not found");
            return;
        }

        Console.WriteLine("Enter the word to search : ");
        string enteredWord = Console.ReadLine();      // TC: O(1)
        enteredWord.Trim();     // TC: O(1)

        bool isWordFound = dictionaryTrie.Search(enteredWord);     // TC: O(L), L = length of word
        if (isWordFound)
        {
            Console.WriteLine(enteredWord + " is present in dictionary.");
            return;
        }
        else
        {
            var recommendedWords = dictionaryTrie.SuggestWords(enteredWord, 5);
            if (recommendedWords.Count == 0)
            {
                Console.WriteLine("No word found!");
            }

            Console.WriteLine("No exact search found! Are you trying to search below ones : ");
            foreach (string word in recommendedWords)
            {
                Console.WriteLine(word);
            }
        }
    }
}