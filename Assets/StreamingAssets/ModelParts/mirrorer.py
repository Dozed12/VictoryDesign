from PIL import Image
import glob, os
import sys

def flip_image(image_path, saved_location):
    """
    Flip or mirror the image
    @param image_path: The path to the image to edit
    @param saved_location: Path to save the cropped image
    """
    image_obj = Image.open(image_path)
    rotated_image = image_obj.transpose(Image.FLIP_LEFT_RIGHT)
    rotated_image.save(saved_location)
if __name__ == '__main__':
    os.chdir(os.getcwd() + "/" + sys.argv[1])
    for file in glob.glob("*.png"):
        image = file
        flip_image(image, file)
