using Library.Helper;
using Library.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;
using Microsoft.Data.Sqlite;
using System.Windows.Controls;

namespace Library.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string connectionString = @"Data Source=C:\Database\Library.sqlite;";

        private string _newTitle;
        private string _newAuthor;
        private string _newGenre;
        private decimal _newPrice;
        private bool _newIsAvailable;
        private string _searchText;
        private BookModel _selectedBook;
        private ObservableCollection<BookModel> _books;
        private ICollectionView _booksView;


        public event PropertyChangedEventHandler? PropertyChanged;

        public string NewGenre// proprietate pentru genul cărții introduse
        {
            get => _newGenre;
            set
            {
                if (_newGenre != value)
                {
                    _newGenre = value;
                    //notifica fereastra ca s-a modificat valoarea lui NewGenre
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewGenre"));
                }
            }
        }

        public string NewTitle
        {
            get => _newTitle;
            set
            {
                if (_newTitle != value)
                {
                    _newTitle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewTitle"));
                }
            }
        }

        public string NewAuthor
        {
            get => _newAuthor;
            set
            {
                if (_newAuthor != value)
                {
                    _newAuthor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewAuthor"));
                }
            }
        }

        public decimal NewPrice
        {
            get => _newPrice;
            set
            {
                if (_newPrice != value)
                {
                    _newPrice = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewPrice"));
                }
            }
        }

        public bool NewIsAvailable
        {
            get => _newIsAvailable;
            set
            {
                if (_newIsAvailable != value)
                {
                    _newIsAvailable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewIsAvailable"));
                }
            }
        }

        // textul din bara de căutare; de fiecare dată când se schimbă, reaplicăm filtrul
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchText"));
                    _booksView?.Refresh();
                }
            }
        }

        public BookModel SelectedBook
        {
            get => _selectedBook;
            set
            {
                if (_selectedBook != value)
                {
                    _selectedBook = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedBook"));

                    if (_selectedBook != null)
                    {
                        NewTitle = _selectedBook.Title;
                        NewAuthor = _selectedBook.Author;
                        NewGenre = _selectedBook.Genre;
                        NewPrice = _selectedBook.Price;
                        NewIsAvailable = _selectedBook.IsAvailable;
                    }
                }
            }
        }

        //listă specială de obiecte care anunță automat interfața când se adaugă sau se șterge ceva
        public ObservableCollection<BookModel> Books
        {
            get => _books;
            set
            {
                if (_books != value)
                {
                    _books = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Books"));

                    _booksView = CollectionViewSource.GetDefaultView(_books);
                    _booksView.Filter = FilterBooks;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BooksView"));
                }
            }
        }

        public ICollectionView BooksView => _booksView;

        public ICommand AddBookCommand { get; set; }//comanda care se execută când utilizatorul apasă un buton pentru a adăuga o carte
        public ICommand DeleteBookCommand { get; set; }
        public ICommand EditBookCommand { get; set; }

        public MainWindowViewModel()
        {
            AddBookCommand = new RelayCommand(AddBookCommandExecute);
            DeleteBookCommand = new RelayCommand(DeleteBookCommandExecute);
            EditBookCommand = new RelayCommand(EditBookCommandExecute);
            LoadBooks();
        }

        // funcția de filtrare: verifică dacă titlul, autorul sau genul conțin textul căutat
        private bool FilterBooks(object obj)
        {
            if (obj is not BookModel book)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            string search = SearchText.Trim().ToLower();

            bool matchesTitle = book.Title?.ToLower().Contains(search) == true;
            bool matchesAuthor = book.Author?.ToLower().Contains(search) == true;
            bool matchesGenre = book.Genre?.ToLower().Contains(search) == true;

            return matchesTitle || matchesAuthor || matchesGenre;
        }

        public void LoadBooks()// metodă care încarcă toate cărțile din baza de date în lista Books
        {
            Books = new ObservableCollection<BookModel>();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM BOOKS ";
            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Books.Add(new BookModel
                {
                    BookId = Convert.ToInt32(reader["BOOK_ID"]),
                    Title = Convert.ToString(reader["TITLE"]),
                    Author = Convert.ToString(reader["AUTHOR"]),
                    Genre = Convert.ToString(reader["GENRE"]),
                    Price = Convert.ToDecimal(reader["PRICE"]),
                    IsAvailable = Convert.ToBoolean(reader["IS_AVAILABLE"]),

                });
            }
        }

        public void AddBook()// metodă care adaugă o nouă carte în baza de date
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string query = @"INSERT INTO BOOKS(TITLE,AUTHOR,GENRE,PRICE,IS_AVAILABLE)
                                 VALUES($title, $author, $genre, $price, $isAvailable)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("$title", NewTitle);
            command.Parameters.AddWithValue("$author", NewAuthor);
            command.Parameters.AddWithValue("$genre", NewGenre);
            command.Parameters.AddWithValue("$price", NewPrice);
            command.Parameters.AddWithValue("$isAvailable", NewIsAvailable);
            using var reader = command.ExecuteReader();
        }


        public void AddBookCommandExecute()
        {
            AddBook();
            LoadBooks();
            ClearForm();
        }

        public void UpdateBook(BookModel book)// metodă care actualizează o carte existentă în baza de date
        {
            if (book == null)
                return;

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string query = @"UPDATE BOOKS
                              SET TITLE = $title,
                                  AUTHOR = $author,
                                  GENRE = $genre,
                                  PRICE = $price,
                                  IS_AVAILABLE = $isAvailable
                              WHERE BOOK_ID = $id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("$title", NewTitle);
            command.Parameters.AddWithValue("$author", NewAuthor);
            command.Parameters.AddWithValue("$genre", NewGenre);
            command.Parameters.AddWithValue("$price", NewPrice);
            command.Parameters.AddWithValue("$isAvailable", NewIsAvailable);
            command.Parameters.AddWithValue("$id", book.BookId);

            command.ExecuteNonQuery();
        }

        public void EditBookCommandExecute()
        {
            if (SelectedBook == null)
                return;

            UpdateBook(SelectedBook);
            LoadBooks();
            ClearForm();
        }

        public void DeleteBook(BookModel book)
        {
            if (book == null)
                return;

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string query = @"DELETE FROM BOOKS
                     WHERE BOOK_ID = $id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("$id", book.BookId);

            command.ExecuteNonQuery();

            Books.Remove(book);
        }

        public void DeleteBookCommandExecute()
        {
            if (SelectedBook != null)
            {
                DeleteBook(SelectedBook);
                ClearForm();
            }
        }

        public void ClearForm()
        {
            SelectedBook = null;
            NewTitle = null;
            NewAuthor = null;
            NewGenre = null;
            NewPrice = 0;
            NewIsAvailable = false;
        }
    }
}