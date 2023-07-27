from torch.nn import CosineSimilarity

from person import Person, PersonData

import configuration as cfg
import dataManager as dataMgr
import embedder as emb


# noinspection PyBroadException
def fill_embedding_table_from_files():
    """
    Fills the embedding table with data from the received directory.
    """
    global __embeddings_table

    try:
        __embeddings_table = []
        guid_list = dataMgr.get_guid_list()
        if len(guid_list) == 0:
            print('Warning> No person has been saved in the system yet.')
        else:
            for guid in guid_list:
                add_person_by_guid(guid)
    except Exception:
        print(Exception)


# noinspection PyBroadException
def add_person(person: Person):
    try:
        if len(person.data) > 0:
            __embeddings_table.append(person)
            print(f'Info> Person {person.guid} added in embedding table.')
        else:
            print(f'Error> Data of Person {person.guid} is empty!')
    except Exception:
        print(Exception)
        pass


# noinspection PyBroadException
def add_person_by_guid(guid: str):
    """
    Add a new person to the embedding table by guid.

    :param guid: guid of the person being added
    """

    try:
        person_data_array = dataMgr.get_aligned_faces_by_guid(guid)
        if len(person_data_array) != 0:
            person = Person(guid)
            for person_data in person_data_array:
                print(f'Info> {guid} photo found.')
                embedding = emb.get_embedding(person_data.face_image)
                if embedding is not None:
                    person.add_image(embedding, person_data.path, person_data.face_image)
                else:
                    print('Error> Embedding is None!!!')
            add_person(person)
        else:
            print(f'Warning> Person {guid} does not have a photo!')
    except Exception:
        print(Exception)
        pass


# noinspection PyBroadException
def similarity_between_persons(person1: Person, person2: Person):
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
                similarity = abs(__cos(pd1.embedding, pd2.embedding).item())
                if similarity > max_similarity:
                    max_similarity = similarity
                    most_similar_data = pd2
        return max_similarity, most_similar_data
    except Exception:
        print(Exception)
        return None


# noinspection PyBroadException
def most_similar_person(compared_person: Person):
    """
    Find and return from the embedding table the person who is most similar to the person being compared.

    :param compared_person: the person relative to which the most similar person from the table is located
    :return: most similar person from the table, data of most similar person and max similarity value
    """

    try:
        max_similarity: float = 0.0
        most_similar: Person = compared_person
        most_similar_data: PersonData = compared_person.data[0]
        for person in __embeddings_table:
            similarity, similar_data = similarity_between_persons(compared_person, person)
            if similarity > max_similarity:
                max_similarity = similarity
                most_similar = person
                most_similar_data = similar_data
        return most_similar, most_similar_data, max_similarity
    except Exception:
        print(Exception)
        return None, None, None


def print_embeddings_table():
    """displays the contents of the embedding table."""

    for person in __embeddings_table:
        print('person name: ', person.guid, ' embedding count: ', len(person.data))


def check_max_count(person):
    return len(person.data) == cfg.max_photo_count


def get_person_by_guid(guid: str):
    """
    :param guid: guid of the person
    :return: with this guid or None.
    """

    return next((person for person in __embeddings_table if person.guid == guid), None)


def add_person_data(guid: str, person_data: PersonData):
    person: Person = get_person_by_guid(guid)
    if person is not None:
        person.add_data(person_data)
    else:
        person = Person(guid)
        person.add_data(person_data)
        add_person(person)


def compare_persons():
    for person1 in __embeddings_table:
        for person2 in __embeddings_table:
            maximum, s = similarity_between_persons(person1, person2)
            print(f"person {person1.guid} - person {person2.guid} - {maximum}")


__embeddings_table = []
__similarity_table = []
__cos = CosineSimilarity(eps=1e-6)
print('Info> Embeddings table was created.')
fill_embedding_table_from_files()
