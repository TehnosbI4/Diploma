# Original code
# https://github.com/ZhaoJ9014/face.evoLVe.PyTorch/blob/master/util/extract_feature_v1.py
import numpy
import torch
import torch.nn.functional as f
import torchvision.transforms as transforms
import configuration as cfg
from backbone import Backbone


__transform = transforms.Compose(
    [
        transforms.ToPILImage(),
        transforms.Resize([int(128 * cfg.face_size[0] / 112), int(128 * cfg.face_size[0] / 112)], ),
        transforms.CenterCrop([cfg.face_size[0], cfg.face_size[1]]),
        transforms.ToTensor(),
        transforms.Normalize(mean=[0.5, 0.5, 0.5], std=[0.5, 0.5, 0.5]),
    ],
)
__backbone = Backbone(cfg.face_size)
__backbone.load_state_dict(torch.load(cfg.model_path, map_location=torch.device('cpu')))
__backbone.to(cfg.device)
__backbone.eval()
print(f'Info> Embedder with input size {cfg.face_size} and device {cfg.device} was created')


# noinspection PyBroadException
def get_embedding(image: numpy.ndarray):
    """
    Calculate and return embedding.

    :param image: input image in numpy.ndarray format
    """

    try:
        if image.shape[0] == cfg.face_size[0] and image.shape[1] == cfg.face_size[1]:
            with torch.no_grad():
                print('Process> embedding calculation started...', sep='', end='')
                image = __transform(image)
                image = torch.unsqueeze(image, 0)
                embedding = f.normalize(__backbone(image.to(cfg.device))).cpu()
                print(' Done!')
            return embedding
        else:
            print('Error> Incorrect image size!')
            return None
    except Exception:
        print(Exception)
        return None


# noinspection PyBroadException
def get_embeddings_list(images):
    embeddings = []
    try:
        for image in images:
            embeddings.append(get_embedding(image))
        return embeddings
    except Exception:
        print(Exception)
        return None
