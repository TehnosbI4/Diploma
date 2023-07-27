import numpy
import torch


class PersonData:
    def __init__(self, embedding=None, path=None, face_image=None):
        """
        Class describing a person data.

        :param embedding: embedding from photo of this person
        :param path: path from proto of this person
        :param face_image: face image of this person
        """

        self.embedding = embedding
        self.path = path
        self.face_image = face_image


class Person:
    def __init__(self, guid: str):
        """
        Class describing a person.

        :param guid: describing the name of the folder where the photos of the person's
        """

        self.guid = guid
        self.data: list[PersonData] = []

    def add_image(self, embedding: torch.Tensor, path: str, face_image: numpy.ndarray):
        """
        Add embedding,image, path and embedding for person

        :param face_image: face image of person
        :param embedding: embedding from photo of this person
        :param path: path from proto of this person
        """

        pd = PersonData(embedding, path, face_image)
        print(f'Info> Added image for {self.guid}')
        self.data.append(pd)

    def add_data(self, data: PersonData):
        """
        Add PersonData to Person

        :param data: personData object
        """
        print(f'Info> Added person data for {self.guid}')
        self.data.append(data)
