from torch.nn import CosineSimilarity

from dataManager import DataManager
from embedder import Embedder
from person import Person, PersonData


# noinspection PyBroadException
class EmbeddingsTable:
    def __init__(self, embedder: Embedder, data_manager: DataManager, max_photo_count: int = 5):
        """
        The embedding table class. Stores embedding in memory to speed up the calculation of the similarity.

        :param max_photo_count: maximum number of photos for one person
        :param embedder: embedder for calculating embeddings
        :param data_manager: DataManager object for manipulation with data
        """

        self.__max_photo_count = max_photo_count
        self.__embeddings_table = []
        self.__similarity_table = []
        self.__embedder: Embedder = embedder
        self.__data_manager: DataManager = data_manager
        self.__cos = CosineSimilarity(eps=1e-6)
        print('Info> Embeddings table was created.')
        self.fill_embedding_table_from_files()

    def fill_embedding_table_from_files(self):
        """
        Fills the embedding table with data from the received directory.
        """

        try:
            self.__embeddings_table = []
            guid_list = self.__data_manager.get_guid_list()
            if len(guid_list) == 0:
                print('Warning> No person has been saved in the system yet.')
            else:
                for guid in guid_list:
                    self.add_person_by_guid(guid)
        except Exception:
            print(Exception)

    def add_person(self, person: Person):
        try:
            if len(person.data) > 0:
                self.__embeddings_table.append(person)
                print(f'Info> Person {person.guid} added in embedding table.')
            else:
                print(f'Error> Data of Person {person.guid} is empty!')
        except Exception:
            print(Exception)
            pass

    def add_person_by_guid(self, guid: str):
        """
        Add a new person to the embedding table by guid.

        :param guid: guid of the person being added
        """

        try:
            person_data_array = self.__data_manager.get_aligned_image_by_guid(guid)
            if len(person_data_array) == 0:
                print(f'Warning> Person {guid} does not have a photo!')
            else:
                person = Person(guid)
                for person_data in person_data_array:
                    print(f'Info> {guid} photo found.')
                    embedding = self.__embedder.get_embedding(person_data.face_image)
                    if embedding is not None:
                        person.add_image(embedding, person_data.path, person_data.face_image)
                    else:
                        print('Error> Embedding is None!!!')
                self.add_person(person)
        except Exception:
            print(Exception)
            pass

    def similarity_between_persons(self, person1: Person, person2: Person):
        """
        Find and return minimal cosine from 0 to 1 cosine distance between two persons.

        :param person1: first person
        :param person2: second person
        :return: max similarity from 0 to 1 between two persons and most similar PersonData of second person
        """

        max_similarity: float = 0.0
        most_similar_data = None
        try:
            for pd1 in person1.data:
                for pd2 in person2.data:
                    similarity = abs(self.__cos(pd1.embedding, pd2.embedding).item())
                    if similarity > max_similarity:
                        max_similarity = similarity
                        most_similar_data = pd2
            return max_similarity, most_similar_data
        except Exception:
            print(Exception)
            return None

    def most_similar_person(self, compared_person: Person):
        """
        Find and return from the embedding table the person who is most similar to the person being compared.

        :param compared_person: the person relative to which the most similar person from the table is located
        :return: most similar person from the table, data of most similar person and max similarity value
        """

        try:
            max_similarity: float = 0.0
            most_similar_person: Person = compared_person
            most_similar_data: PersonData = compared_person.data[0]
            for person in self.__embeddings_table:
                similarity, similar_data = self.similarity_between_persons(compared_person, person)
                if similarity > max_similarity:
                    max_similarity = similarity
                    most_similar_person = person
                    most_similar_data = similar_data
            return most_similar_person, most_similar_data, max_similarity
        except Exception:
            print(Exception)
            return None, None, None

    def print_embeddings_table(self):
        """displays the contents of the embedding table."""

        for person in self.__embeddings_table:
            print('person name: ', person.guid, ' embedding count: ', len(person.data))

    def check_max_count(self, person):
        return len(person.data) == self.__max_photo_count
